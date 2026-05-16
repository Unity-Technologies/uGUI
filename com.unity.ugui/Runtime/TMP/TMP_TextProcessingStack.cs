using System;
using System.Diagnostics;
using UnityEngine;

namespace TMPro
{
    /// <summary>
    /// Structure used to track basic XML tags which are binary (on / off)
    /// </summary>
    public struct TMP_FontStyleStack
    {
        public byte bold;
        public byte italic;
        public byte underline;
        public byte strikethrough;
        public byte highlight;
        public byte superscript;
        public byte subscript;
        public byte uppercase;
        public byte lowercase;
        public byte smallcaps;

        /// <summary>
        /// Clear the basic XML tag stack.
        /// </summary>
        public void Clear()
        {
            bold = 0;
            italic = 0;
            underline = 0;
            strikethrough = 0;
            highlight = 0;
            superscript = 0;
            subscript = 0;
            uppercase = 0;
            lowercase = 0;
            smallcaps = 0;
        }

        public byte Add(FontStyles style)
        {
            switch (style)
            {
                case FontStyles.Bold:
                    bold++;
                    return bold;
                case FontStyles.Italic:
                    italic++;
                    return italic;
                case FontStyles.Underline:
                    underline++;
                    return underline;
                case FontStyles.UpperCase:
                    uppercase++;
                    return uppercase;
                case FontStyles.LowerCase:
                    lowercase++;
                    return lowercase;
                case FontStyles.Strikethrough:
                    strikethrough++;
                    return strikethrough;
                case FontStyles.Superscript:
                    superscript++;
                    return superscript;
                case FontStyles.Subscript:
                    subscript++;
                    return subscript;
                case FontStyles.Highlight:
                    highlight++;
                    return highlight;
            }

            return 0;
        }

        public byte Remove(FontStyles style)
        {
            switch (style)
            {
                case FontStyles.Bold:
                    if (bold > 1)
                        bold--;
                    else
                        bold = 0;
                    return bold;
                case FontStyles.Italic:
                    if (italic > 1)
                        italic--;
                    else
                        italic = 0;
                    return italic;
                case FontStyles.Underline:
                    if (underline > 1)
                        underline--;
                    else
                        underline = 0;
                    return underline;
                case FontStyles.UpperCase:
                    if (uppercase > 1)
                        uppercase--;
                    else
                        uppercase = 0;
                    return uppercase;
                case FontStyles.LowerCase:
                    if (lowercase > 1)
                        lowercase--;
                    else
                        lowercase = 0;
                    return lowercase;
                case FontStyles.Strikethrough:
                    if (strikethrough > 1)
                        strikethrough--;
                    else
                        strikethrough = 0;
                    return strikethrough;
                case FontStyles.Highlight:
                    if (highlight > 1)
                        highlight--;
                    else
                        highlight = 0;
                    return highlight;
                case FontStyles.Superscript:
                    if (superscript > 1)
                        superscript--;
                    else
                        superscript = 0;
                    return superscript;
                case FontStyles.Subscript:
                    if (subscript > 1)
                        subscript--;
                    else
                        subscript = 0;
                    return subscript;
            }

            return 0;
        }
    }


    /// <summary>
    /// Structure used to track XML tags of various types.
    /// </summary>
    /// <typeparam name="T">The element type of the stack.</typeparam>
    [DebuggerDisplay("Item count = {m_Count}")]
    public struct TMP_TextProcessingStack<T>
    {
        public T[] itemStack;
        public int index;

        T m_DefaultItem;
        int m_Capacity;
        int m_RolloverSize;
        int m_Count;

        const int k_DefaultCapacity = 4;


        /// <summary>
        /// Constructor to create a new item stack.
        /// </summary>
        /// <param name="stack">Backing array that stores stack entries; capacity equals this array length.</param>
        /// <remarks>
        /// Initializes internal counters so the stack starts empty while reusing the provided buffer to avoid an extra allocation when the caller already sized the array.
        /// </remarks>
        public TMP_TextProcessingStack(T[] stack)
        {
            itemStack = stack;
            m_Capacity = stack.Length;
            index = 0;
            m_RolloverSize = 0;

            m_DefaultItem = default(T);
            m_Count = 0;
        }


        /// <summary>
        /// Constructor for a new item stack with the given capacity.
        /// </summary>
        /// <param name="capacity">Number of slots to allocate in the internal item array before any push operations.</param>
        /// <remarks>
        /// Allocates <paramref name="capacity"/> entries up front so nested rich-text tags can push state without resizing during typical markup depth.
        /// </remarks>
        public TMP_TextProcessingStack(int capacity)
        {
            itemStack = new T[capacity];
            m_Capacity = capacity;
            index = 0;
            m_RolloverSize = 0;

            m_DefaultItem = default(T);
            m_Count = 0;
        }


        public TMP_TextProcessingStack(int capacity, int rolloverSize)
        {
            itemStack = new T[capacity];
            m_Capacity = capacity;
            index = 0;
            m_RolloverSize = rolloverSize;

            m_DefaultItem = default(T);
            m_Count = 0;
        }


        /// <summary>
        /// Gets the number of items currently stored on the stack.
        /// </summary>
        /// <remarks>
        /// Backed by <c>m_Count</c>, which push, pop, and remove operations keep aligned with the logical depth of active rich-text scopes.
        /// </remarks>
        public int Count
        {
            get { return m_Count; }
        }


        /// <summary>
        /// Returns the current item on the stack.
        /// </summary>
        public T current
        {
            get
            {
                if (index > 0)
                    return itemStack[index - 1];

                return itemStack[0];
            }
        }


        /// <summary>
        /// Gets or sets the rollover size used when the stack wraps in circular-buffer mode.
        /// </summary>
        /// <remarks>
        /// When set to zero, pushes grow the backing array; when positive, the stack index wraps modulo the rollover size to cap memory for unbounded tag nesting.
        /// </remarks>
        public int rolloverSize
        {
            get { return m_RolloverSize; }
            set
            {
                m_RolloverSize = value;

                //if (m_Capacity < m_RolloverSize)
                //    Array.Resize(ref itemStack, m_RolloverSize);
            }
        }


        /// <summary>
        /// Set stack elements to default item.
        /// </summary>
        /// <param name="stack">The stack of elements.</param>
        /// <param name="item"></param>
        internal static void SetDefault(TMP_TextProcessingStack<T>[] stack, T item)
        {
            for (int i = 0; i < stack.Length; i++)
                stack[i].SetDefault(item);
        }


        /// <summary>
        /// Clears and resets stack to first item.
        /// </summary>
        /// <remarks>
        /// Resets the logical count and index so the next push or set default rebuilds state from scratch without reallocating the backing array reference.
        /// </remarks>
        public void Clear()
        {
            index = 0;
            m_Count = 0;
        }


        /// <summary>
        /// Sets the first item on the stack and reset index.
        /// </summary>
        /// <param name="item">Baseline value written to index zero and returned when the stack would otherwise be empty.</param>
        /// <remarks>
        /// Lazily allocates the backing array when null, then sets index to one so subsequent adds append rich-text overrides above the default style.
        /// </remarks>
        public void SetDefault(T item)
        {
            if (itemStack == null)
            {
                m_Capacity = k_DefaultCapacity;
                itemStack = new T[m_Capacity];
                m_DefaultItem = default(T);
            }

            itemStack[0] = item;
            index = 1;
            m_Count = 1;
        }


        /// <summary>
        /// Adds a new item to the stack.
        /// </summary>
        /// <param name="item">Value to store at the current write index when capacity allows further growth.</param>
        /// <remarks>
        /// Writes only when index is below the backing array length; this legacy path does not expand storage, unlike <see cref="Push"/>.
        /// </remarks>
        public void Add(T item)
        {
            if (index < itemStack.Length)
            {
                itemStack[index] = item;
                index += 1;
            }
        }


        /// <summary>
        /// Retrieves an item from the stack.
        /// </summary>
        /// <returns>The value exposed after decrementing the stack pointer, or the default slot when collapsing to one entry.</returns>
        /// <remarks>
        /// Decrements both index and count, clamping to one level so <see cref="current"/> always has a defined fallback entry at index zero.
        /// </remarks>
        public T Remove()
        {
            index -= 1;
            m_Count -= 1;

            if (index <= 0)
            {
                m_Count = 0;
                index = 1;
                return itemStack[0];

            }

            return itemStack[index - 1];
        }

        public void Push(T item)
        {
            if (index == m_Capacity)
            {
                m_Capacity *= 2;
                if (m_Capacity == 0)
                    m_Capacity = k_DefaultCapacity;

                Array.Resize(ref itemStack, m_Capacity);
            }

            itemStack[index] = item;

            if (m_RolloverSize == 0)
            {
                index += 1;
                m_Count += 1;
            }
            else
            {
                index = (index + 1) % m_RolloverSize;
                m_Count = m_Count < m_RolloverSize ? m_Count + 1 : m_RolloverSize;
            }

        }

        public T Pop()
        {
            if (index == 0 && m_RolloverSize == 0)
                return default(T);

            if (m_RolloverSize == 0)
                index -= 1;
            else
            {
                index = (index - 1) % m_RolloverSize;
                index = index < 0 ? index + m_RolloverSize : index;
            }

            T item = itemStack[index];
            itemStack[index] = m_DefaultItem;

            m_Count = m_Count > 0 ? m_Count - 1 : 0;

            return item;
        }

        /// <summary>
        /// Returns the top item on the stack without removing it.
        /// </summary>
        /// <returns>The most recently pushed value, or the baseline entry at index zero when nothing has been pushed yet.</returns>
        /// <remarks>
        /// Non-destructive read used when parsers need the active style without mutating the stack during lookahead.
        /// </remarks>
        public T Peek()
        {
            if (index == 0)
                return m_DefaultItem;

            return itemStack[index - 1];
        }


        /// <summary>
        /// Function to retrieve the current item from the stack.
        /// </summary>
        /// <returns>The current item T from the stack.</returns>
        public T CurrentItem()
        {
            if (index > 0)
                return itemStack[index - 1];

            return itemStack[0];
        }


        /// <summary>
        /// Retrieves the previous item without affecting the stack.
        /// </summary>
        /// <returns>The value one level below the top when at least two entries exist; otherwise the first slot.</returns>
        /// <remarks>
        /// Useful when comparing the prior style to the active style while leaving the stack unchanged for subsequent characters.
        /// </remarks>
        public T PreviousItem()
        {
            if (index > 1)
                return itemStack[index - 2];

            return itemStack[0];
        }
    }
}

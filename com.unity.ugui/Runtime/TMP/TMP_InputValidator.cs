using UnityEngine;
using System.Collections;


namespace TMPro
{
    /// <summary>
    /// Custom text input validator where user can implement their own custom character validation.
    /// </summary>
    [System.Serializable]
    public abstract class TMP_InputValidator : ScriptableObject
    {
        /// <summary>
        /// Customs text input validation function.
        /// </summary>
        /// <param name="text">The original text</param>
        /// <param name="pos">The position in the string to add the caharcter</param>
        /// <param name="ch">The character to add</param>
        /// <returns>The character added</returns>
        public abstract char Validate(ref string text, ref int pos, char ch);
    }
}

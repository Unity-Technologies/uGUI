using UnityEngine;
using System.Collections;


namespace TMPro
{
    public enum CaretPosition { None, Left, Right }

    /// <summary>
    /// Structure which contains the character index and position of caret relative to the character.
    /// </summary>
    public struct CaretInfo
    {
        public int index;
        public CaretPosition position;

        public CaretInfo(int index, CaretPosition position)
        {
            this.index = index;
            this.position = position;
        }
    }

    public static class TMP_TextUtilities
    {
        // Note: this variable exists to avoid repeated allocations in IsIntersectingRectTransform
        private static readonly Vector3[] m_rectWorldCorners = new Vector3[4];


        // TEXT INPUT COMPONENT RELATED FUNCTIONS

        /// <summary>
        ///
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static CaretInfo GetCursorInsertionIndex(TMP_Text textComponent, Vector3 position, Camera camera)
        //{
        //    int index = TMP_TextUtilities.FindNearestCharacter(textComponent, position, camera, false);

        //    RectTransform rectTransform = textComponent.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

        //    // Get Bottom Left and Top Right position of the current character
        //    Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
        //    //Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //    Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
        //    //Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //    float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

        //    if (insertPosition < 0.5f)
        //        return new CaretInfo(index, CaretPosition.Left);
        //    else
        //        return new CaretInfo(index, CaretPosition.Right);
        //}


        /// <summary>
        /// Method returning the index of the character whose origin is closest to the cursor.
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>Character index for cursor insertion.</returns>
        /// <remarks>
        /// Combines nearest-character search with horizontal placement within the glyph so caret insertion lands before or after the glyph center when clicking.
        /// </remarks>
        public static int GetCursorIndexFromPosition(TMP_Text textComponent, Vector3 position, Camera camera)
        {
            int index = TMP_TextUtilities.FindNearestCharacter(textComponent, position, camera, false);

            RectTransform rectTransform = textComponent.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

            // Get Bottom Left and Top Right position of the current character
            Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
            Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);

            float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

            if (insertPosition < 0.5f)
                return index;
            else
                return index + 1;

        }


        /// <summary>
        /// Function returning the index of the character whose origin is closest to the cursor.
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="cursor">The position of the cursor insertion position relative to the position.</param>
        /// <returns></returns>
        //public static int GetCursorIndexFromPosition(TMP_Text textComponent, Vector3 position, Camera camera, out CaretPosition cursor)
        //{
        //    int index = TMP_TextUtilities.FindNearestCharacter(textComponent, position, camera, false);

        //    RectTransform rectTransform = textComponent.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

        //    // Get Bottom Left and Top Right position of the current character
        //    Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
        //    Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);

        //    float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

        //    if (insertPosition < 0.5f)
        //    {
        //        cursor = CaretPosition.Left;
        //        return index;
        //    }
        //    else
        //    {
        //        cursor = CaretPosition.Right;
        //        return index;
        //    }
        //}


        /// <summary>
        /// Method returning the index of the character whose origin is closest to the cursor.
        /// </summary>
        /// <param name="textComponent">A reference to the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="cursor">The position of the cursor insertion position relative to the position.</param>
        /// <returns>The index of the character whose origin is closest to the cursor.</returns>
        /// <remarks>
        /// Resolves the line first, then picks the nearest glyph on that line and splits left versus right using the midpoint between the glyph corners.
        /// </remarks>
        public static int GetCursorIndexFromPosition(TMP_Text textComponent, Vector3 position, Camera camera, out CaretPosition cursor)
        {
            int line = FindNearestLine(textComponent, position, camera);

            // Return if text doesn't contain any lines of text.
            if (line == -1)
            {
                cursor = CaretPosition.Left;
                return 0;
            }

            int index = FindNearestCharacterOnLine(textComponent, position, line, camera, false);

            // Special handling if line contains only one character.
            if (textComponent.textInfo.lineInfo[line].characterCount == 1)
            {
                cursor = CaretPosition.Left;
                return index;
            }

            RectTransform rectTransform = textComponent.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            TMP_CharacterInfo cInfo = textComponent.textInfo.characterInfo[index];

            // Get Bottom Left and Top Right position of the current character
            Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
            Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);

            float insertPosition = (position.x - bl.x) / (tr.x - bl.x);

            if (insertPosition < 0.5f)
            {
                cursor = CaretPosition.Left;
                return index;
            }
            else
            {
                cursor = CaretPosition.Right;
                return index;
            }
        }


        /// <summary>
        /// Method returning the line nearest to the position.
        /// </summary>
        /// <param name="text">TMP instance whose <see cref="TMP_Text.textInfo"/> line metrics define bounds.</param>
        /// <param name="position">Pointer or world position converted through the RectTransform before comparison.</param>
        /// <param name="camera">Camera used to project screen points; null when the canvas is screen-space overlay.</param>
        /// <returns>Zero-based line index when inside vertical bounds, otherwise the closest line by vertical distance.</returns>
        /// <remarks>
        /// Prefers the line whose ascender and descender contain the point, falling back to whichever line minimizes vertical distance when the point sits between rows.
        /// </remarks>
        public static int FindNearestLine(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            float distance = Mathf.Infinity;
            int closest = -1;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = text.textInfo.lineInfo[i];

                float ascender = rectTransform.TransformPoint(new Vector3(0, lineInfo.ascender, 0)).y;
                float descender = rectTransform.TransformPoint(new Vector3(0, lineInfo.descender, 0)).y;

                if (ascender > position.y && descender < position.y)
                {
                    //Debug.Log("Position is on line " + i);
                    return i;
                }

                float d0 = Mathf.Abs(ascender - position.y);
                float d1 = Mathf.Abs(descender - position.y);

                float d = Mathf.Min(d0, d1);
                if (d < distance)
                {
                    distance = d;
                    closest = i;
                }
            }

            //Debug.Log("Closest line to position is " + closest);
            return closest;
        }


        /// <summary>
        /// Method returning the nearest character to position on a given line.
        /// </summary>
        /// <param name="text">TMP instance whose character metrics are used for the search.</param>
        /// <param name="position">Position converted into the RectTransform space before distance tests.</param>
        /// <param name="line">Zero-based line index previously returned from <see cref="FindNearestLine"/>.</param>
        /// <param name="camera">Camera used for screen to world conversion; null for overlay canvases.</param>
        /// <param name="visibleOnly">Skips characters marked not visible when true, keeping invisible placeholders out of hit results.</param>
        /// <returns>Character index within <see cref="TMP_Text.textInfo"/> constrained to the requested line.</returns>
        /// <remarks>
        /// Chooses the glyph rectangle that contains the point when possible; otherwise it minimizes squared distance to the quad edges for caret placement.
        /// </remarks>
        public static int FindNearestCharacterOnLine(TMP_Text text, Vector3 position, int line, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            int firstCharacter = text.textInfo.lineInfo[line].firstCharacterIndex;
            int lastCharacter = text.textInfo.lineInfo[line].lastCharacterIndex;

            float distanceSqr = Mathf.Infinity;
            int closest = lastCharacter;

            for (int i = firstCharacter; i < lastCharacter; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible) continue;

                // Ignore Carriage Returns <CR>
                if (cInfo.character == '\r')
                    continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                {
                    closest = i;
                    break;
                }

                // Find the closest corner to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
            }
            return closest;
        }


        /// <summary>
        /// Method used to determine if the position intersects with the RectTransform.
        /// </summary>
        /// <param name="rectTransform">A reference to the RectTranform of the text object.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>Whether the transformed point lies inside the world-space rectangle described by the four corners.</returns>
        /// <remarks>
        /// Projects the point through <see cref="ScreenPointToWorldPointInRectangle"/> then tests against the cached world corners from <see cref="RectTransform.GetWorldCorners"/>.
        /// </remarks>
        public static bool IsIntersectingRectTransform(RectTransform rectTransform, Vector3 position, Camera camera)
        {
            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            rectTransform.GetWorldCorners(m_rectWorldCorners);

            if (PointIntersectRectangle(position, m_rectWorldCorners[0], m_rectWorldCorners[1], m_rectWorldCorners[2], m_rectWorldCorners[3]))
            {
                return true;
            }

            return false;
        }


        // CHARACTER HANDLING

        /// <summary>
        /// Method returning the index of the character at the given position (if any).
        /// Returns @@-1@@ if no character is found at the specified position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which is rendering the text or whichever one might be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns>Character index if intersecting, -1 otherwise.</returns>
        /// <remarks>
        /// Iterates every character quad until one contains the point, skipping invisible glyphs when <paramref name="visibleOnly"/> is enabled for lightweight hit tests.
        /// </remarks>
        public static int FindIntersectingCharacter(TMP_Text text, Vector3 position, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

            }
            return -1;
        }


        /// <summary>
        /// Function returning the index of the character at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns></returns>
        //public static int FindIntersectingCharacter(TextMeshPro text, Vector3 position, Camera camera, bool visibleOnly)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.characterCount; i++)
        //    {
        //        // Get current character info.
        //        TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
        //        if ((visibleOnly && !cInfo.isVisible) || (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay))
        //            continue;

        //        // Get Bottom Left and Top Right position of the current character
        //        Vector3 bl = textTransform.TransformPoint(cInfo.bottomLeft);
        //        Vector3 tl = textTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //        Vector3 tr = textTransform.TransformPoint(cInfo.topRight);
        //        Vector3 br = textTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //        if (PointIntersectRectangle(position, bl, tl, tr, br))
        //            return i;

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Method to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TMP Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns>Zero-based character index in <see cref="TMP_Text.textInfo"/> whose quad is closest by edge distance.</returns>
        /// <remarks>
        /// Returns immediately when a quad contains the point; otherwise it tracks the minimum distance-to-edge metric across all candidate glyphs.
        /// </remarks>
        public static int FindNearestCharacter(TMP_Text text, Vector3 position, Camera camera, bool visibleOnly)
        {
            RectTransform rectTransform = text.rectTransform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.characterCount; i++)
            {
                // Get current character info.
                TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
                if (visibleOnly && !cInfo.isVisible) continue;

                // Get Bottom Left and Top Right position of the current character
                Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
                Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
                Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
                Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

                if (PointIntersectRectangle(position, bl, tl, tr, br))
                    return i;

                // Find the closest corner to position.
                float dbl = DistanceToLine(bl, tl, position);
                float dtl = DistanceToLine(tl, tr, position);
                float dtr = DistanceToLine(tr, br, position);
                float dbr = DistanceToLine(br, bl, position);

                float d = dbl < dtl ? dbl : dtl;
                d = d < dtr ? d : dtr;
                d = d < dbr ? d : dbr;

                if (distanceSqr > d)
                {
                    distanceSqr = d;
                    closest = i;
                }
            }

            return closest;
        }


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns>Index of the nearest character.</returns>
        //public static int FindNearestCharacter(TextMeshProUGUI text, Vector3 position, Camera camera, bool visibleOnly)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.characterCount; i++)
        //    {
        //        // Get current character info.
        //        TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
        //        if ((visibleOnly && !cInfo.isVisible) || (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay))
        //            continue;

        //        // Get Bottom Left and Top Right position of the current character
        //        Vector3 bl = rectTransform.TransformPoint(cInfo.bottomLeft);
        //        Vector3 tl = rectTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //        Vector3 tr = rectTransform.TransformPoint(cInfo.topRight);
        //        Vector3 br = rectTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //        if (PointIntersectRectangle(position, bl, tl, tr, br))
        //            return i;

        //        // Find the closest corner to position.
        //        float dbl = DistanceToLine(bl, tl, position);
        //        float dtl = DistanceToLine(tl, tr, position);
        //        float dtr = DistanceToLine(tr, br, position);
        //        float dbr = DistanceToLine(br, bl, position);

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //    }

        //    //Debug.Log("Returning nearest character at index: " + closest);

        //    return closest;
        //}


        /// <summary>
        /// Function to find the nearest character to position.
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <param name="visibleOnly">Only check for visible characters.</param>
        /// <returns>Index of the nearest character.</returns>
        //public static int FindNearestCharacter(TextMeshPro text, Vector3 position, Camera camera, bool visibleOnly)
        //{
        //    Transform textTransform = text.transform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.characterCount; i++)
        //    {
        //        // Get current character info.
        //        TMP_CharacterInfo cInfo = text.textInfo.characterInfo[i];
        //        if ((visibleOnly && !cInfo.isVisible) || (text.OverflowMode == TextOverflowModes.Page && cInfo.pageNumber + 1 != text.pageToDisplay))
        //            continue;

        //        // Get Bottom Left and Top Right position of the current character
        //        Vector3 bl = textTransform.TransformPoint(cInfo.bottomLeft);
        //        Vector3 tl = textTransform.TransformPoint(new Vector3(cInfo.bottomLeft.x, cInfo.topRight.y, 0));
        //        Vector3 tr = textTransform.TransformPoint(cInfo.topRight);
        //        Vector3 br = textTransform.TransformPoint(new Vector3(cInfo.topRight.x, cInfo.bottomLeft.y, 0));

        //        if (PointIntersectRectangle(position, bl, tl, tr, br))
        //            return i;

        //        // Find the closest corner to position.
        //        float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
        //        float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
        //        float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
        //        float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //    }

        //    //Debug.Log("Returning nearest character at index: " + closest);

        //    return closest;
        //}


        // WORD HANDLING
        /// <summary>
        /// Method returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>Zero-based word index when the pointer lies inside the word bounds, or -1 when no word region contains the point.</returns>
        /// <remarks>
        /// Builds axis-aligned bounds per word segment, including multi-line words, and returns on the first intersecting quad.
        /// </remarks>
        public static int FindIntersectingWord(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                float maxAscender = -Mathf.Infinity;
                float minDescender = Mathf.Infinity;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = currentCharInfo.isVisible;

                    // Track maximum Ascender and minimum Descender for each word.
                    maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
                    minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
                        tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
                            tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

                            // Transform coordinates to be relative to transform and account min descender and max ascender.
                            bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                            tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                            br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
                        tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

                        // Transform coordinates to be relative to transform and account min descender and max ascender.
                        bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
                        br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

                        maxAscender = -Mathf.Infinity;
                        minDescender = Mathf.Infinity;

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns>Index of the word if position intersects a word, -1 otherwise.</returns>
        //public static int FindIntersectingWord(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        float maxAscender = -Mathf.Infinity;
        //        float minDescender = Mathf.Infinity;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            // Track maximum Ascender and minimum Descender for each word.
        //            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
        //            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
        //                tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                    tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                    // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                    bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                    tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                    br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = rectTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = rectTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                maxAscender = -Mathf.Infinity;
        //                minDescender = Mathf.Infinity;

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns>Index of the word if position intersects a word, -1 otherwise.</returns>
        //public static int FindIntersectingWord(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        float maxAscender = -Mathf.Infinity;
        //        float minDescender = Mathf.Infinity;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            // Track maximum Ascender and minimum Descender for each word.
        //            maxAscender = Mathf.Max(maxAscender, currentCharInfo.ascender);
        //            minDescender = Mathf.Min(minDescender, currentCharInfo.descender);

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0);
        //                tl = new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0);

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                    tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                    // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                    bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                    tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                    br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0);
        //                tr = new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0);

        //                // Transform coordinates to be relative to transform and account min descender and max ascender.
        //                bl = textTransform.TransformPoint(new Vector3(bl.x, minDescender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(tl.x, maxAscender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(tr.x, maxAscender, 0));
        //                br = textTransform.TransformPoint(new Vector3(br.x, minDescender, 0));

        //                // Reset maxAscender and minDescender for next word segment.
        //                maxAscender = -Mathf.Infinity;
        //                minDescender = Mathf.Infinity;

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }
        //    }

        //    return -1;
        //}


        /// <summary>
        /// Method returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>Zero-based word index whose bounds minimize edge distance when the point is outside every word quad.</returns>
        /// <remarks>
        /// Similar to character picking but aggregates metrics per word region so tooltips can snap to whole tokens instead of individual glyphs.
        /// </remarks>
        public static int FindNearestWord(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.wordCount; i++)
            {
                TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < wInfo.characterCount; j++)
                {
                    int characterIndex = wInfo.firstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    bool isCharacterVisible = currentCharInfo.isVisible;

                    if (isBeginRegion == false && isCharacterVisible)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (wInfo.characterCount == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            // Find the closest line segment to position.
                            float dbl = DistanceToLine(bl, tl, position);
                            float dtl = DistanceToLine(tl, tr, position);
                            float dtr = DistanceToLine(tr, br, position);
                            float dbr = DistanceToLine(br, bl, position);

                            float d = dbl < dtl ? dbl : dtl;
                            d = d < dtr ? d : dtr;
                            d = d < dbr ? d : dbr;

                            if (distanceSqr > d)
                            {
                                distanceSqr = d;
                                closest = i;
                            }
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == wInfo.characterCount - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");
            }

            return closest;
        }

        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindNearestWord(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
        //        float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
        //        float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
        //        float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return closest;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindNearestWord(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.wordCount; i++)
        //    {
        //        TMP_WordInfo wInfo = text.textInfo.wordInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < wInfo.characterCount; j++)
        //        {
        //            int characterIndex = wInfo.firstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            bool isCharacterVisible = characterIndex > text.maxVisibleCharacters ||
        //                                      currentCharInfo.lineNumber > text.maxVisibleLines ||
        //                                     (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) ? false : true;

        //            if (isBeginRegion == false && isCharacterVisible)
        //            {
        //                isBeginRegion = true;

        //                bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (wInfo.characterCount == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == wInfo.characterCount - 1)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //         // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position);
        //        float dtl = DistanceToLine(tl, tr, position);
        //        float dtr = DistanceToLine(tr, br, position);
        //        float dbr = DistanceToLine(br, bl, position);

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return closest;

        //}


        /// <summary>
        /// Method returning the line intersecting the position.
        /// </summary>
        /// <param name="text">The text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>First line whose ascender and descender straddle the vertical position, or -1 when no line contains the point.</returns>
        /// <remarks>
        /// Unlike <see cref="FindNearestLine"/> this returns -1 instead of a closest line when the pointer sits outside all bands.
        /// </remarks>
        public static int FindIntersectingLine(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            int closest = -1;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.lineCount; i++)
            {
                TMP_LineInfo lineInfo = text.textInfo.lineInfo[i];

                float ascender = rectTransform.TransformPoint(new Vector3(0, lineInfo.ascender, 0)).y;
                float descender = rectTransform.TransformPoint(new Vector3(0, lineInfo.descender, 0)).y;

                if (ascender > position.y && descender < position.y)
                {
                    //Debug.Log("Position is on line " + i);
                    return i;
                }
            }

            //Debug.Log("Closest line to position is " + closest);
            return closest;
        }


        /// <summary>
        /// Method returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>Zero-based link index from <see cref="TMP_Text.textInfo"/> when the point hits link geometry, otherwise -1.</returns>
        /// <remarks>
        /// Honors page filters so overflow pages do not report links that belong to hidden sheets.
        /// </remarks>
        public static int FindIntersectingLink(TMP_Text text, Vector3 position, Camera camera)
        {
            Transform rectTransform = text.transform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the word
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    // Check if Link characters are on the current page
                    if (text.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        // If Word is one character
                        if (linkInfo.linkTextLength == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.linkTextLength - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                    // If Word is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return -1;
        }

        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindIntersectingLink(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    Transform rectTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            // Check if Link characters are on the current page
        //            if (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Function returning the index of the Link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindIntersectingLink(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            // Check if Link characters are on the current page
        //            if (text.OverflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return -1;
        //}


        /// <summary>
        /// Method returning the index of the link at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TMP_Text component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns>Zero-based link index minimizing distance to the link bounds when the pointer is near but not inside a region.</returns>
        /// <remarks>
        /// Mirrors the word nearest algorithm but operates on link spans so hover highlights can snap to the closest rich-text anchor.
        /// </remarks>
        public static int FindNearestLink(TMP_Text text, Vector3 position, Camera camera)
        {
            RectTransform rectTransform = text.rectTransform;

            // Convert position into Worldspace coordinates
            ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

            float distanceSqr = Mathf.Infinity;
            int closest = 0;

            for (int i = 0; i < text.textInfo.linkCount; i++)
            {
                TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

                bool isBeginRegion = false;

                Vector3 bl = Vector3.zero;
                Vector3 tl = Vector3.zero;
                Vector3 br = Vector3.zero;
                Vector3 tr = Vector3.zero;

                // Iterate through each character of the link
                for (int j = 0; j < linkInfo.linkTextLength; j++)
                {
                    int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
                    TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
                    int currentLine = currentCharInfo.lineNumber;

                    // Check if Link characters are on the current page
                    if (text.overflowMode == TextOverflowModes.Page && currentCharInfo.pageNumber + 1 != text.pageToDisplay) continue;

                    if (isBeginRegion == false)
                    {
                        isBeginRegion = true;

                        //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

                        bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
                        tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

                        // If Link is one character
                        if (linkInfo.linkTextLength == 1)
                        {
                            isBeginRegion = false;

                            br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                            tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                            // Check for Intersection
                            if (PointIntersectRectangle(position, bl, tl, tr, br))
                                return i;

                            // Find the closest line segment to position.
                            float dbl = DistanceToLine(bl, tl, position);
                            float dtl = DistanceToLine(tl, tr, position);
                            float dtr = DistanceToLine(tr, br, position);
                            float dbr = DistanceToLine(br, bl, position);

                            float d = dbl < dtl ? dbl : dtl;
                            d = d < dtr ? d : dtr;
                            d = d < dbr ? d : dbr;

                            if (distanceSqr > d)
                            {
                                distanceSqr = d;
                                closest = i;
                            }

                        }
                    }

                    // Last Character of Word
                    if (isBeginRegion && j == linkInfo.linkTextLength - 1)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }

                    }
                    // If Link is split on more than one line.
                    else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
                    {
                        isBeginRegion = false;

                        br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
                        tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

                        // Check for Intersection
                        if (PointIntersectRectangle(position, bl, tl, tr, br))
                            return i;

                        // Find the closest line segment to position.
                        float dbl = DistanceToLine(bl, tl, position);
                        float dtl = DistanceToLine(tl, tr, position);
                        float dtr = DistanceToLine(tr, br, position);
                        float dbr = DistanceToLine(br, bl, position);

                        float d = dbl < dtl ? dbl : dtl;
                        d = d < dtr ? d : dtr;
                        d = d < dbr ? d : dbr;

                        if (distanceSqr > d)
                        {
                            distanceSqr = d;
                            closest = i;
                        }
                    }
                }

                //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

            }

            return closest;
        }


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro UGUI component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The scene camera which may be assigned to a Canvas using ScreenSpace Camera or WorldSpace render mode. Set to null is using ScreenSpace Overlay.</param>
        /// <returns></returns>
        //public static int FindNearestLink(TextMeshProUGUI text, Vector3 position, Camera camera)
        //{
        //    RectTransform rectTransform = text.rectTransform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(rectTransform, position, camera, out position);

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = rectTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = rectTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position); // (position - bl).sqrMagnitude;
        //        float dtl = DistanceToLine(tl, tr, position); // (position - tl).sqrMagnitude;
        //        float dtr = DistanceToLine(tr, br, position); // (position - tr).sqrMagnitude;
        //        float dbr = DistanceToLine(br, bl, position); // (position - br).sqrMagnitude;

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }

        //    return closest;
        //}


        /// <summary>
        /// Function returning the index of the word at the given position (if any).
        /// </summary>
        /// <param name="text">A reference to the TextMeshPro component.</param>
        /// <param name="position">Position to check for intersection.</param>
        /// <param name="camera">The camera which is rendering the text object.</param>
        /// <returns></returns>
        //public static int FindNearestLink(TextMeshPro text, Vector3 position, Camera camera)
        //{
        //    Transform textTransform = text.transform;

        //    // Convert position into Worldspace coordinates
        //    ScreenPointToWorldPointInRectangle(textTransform, position, camera, out position);

        //    float distanceSqr = Mathf.Infinity;
        //    int closest = 0;

        //    for (int i = 0; i < text.textInfo.linkCount; i++)
        //    {
        //        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[i];

        //        bool isBeginRegion = false;

        //        Vector3 bl = Vector3.zero;
        //        Vector3 tl = Vector3.zero;
        //        Vector3 br = Vector3.zero;
        //        Vector3 tr = Vector3.zero;

        //        // Iterate through each character of the word
        //        for (int j = 0; j < linkInfo.linkTextLength; j++)
        //        {
        //            int characterIndex = linkInfo.linkTextfirstCharacterIndex + j;
        //            TMP_CharacterInfo currentCharInfo = text.textInfo.characterInfo[characterIndex];
        //            int currentLine = currentCharInfo.lineNumber;

        //            if (isBeginRegion == false)
        //            {
        //                isBeginRegion = true;

        //                bl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.descender, 0));
        //                tl = textTransform.TransformPoint(new Vector3(currentCharInfo.bottomLeft.x, currentCharInfo.ascender, 0));

        //                //Debug.Log("Start Word Region at [" + currentCharInfo.character + "]");

        //                // If Word is one character
        //                if (linkInfo.linkTextLength == 1)
        //                {
        //                    isBeginRegion = false;

        //                    br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                    tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                    // Check for Intersection
        //                    if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                        return i;

        //                    //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //                }
        //            }

        //            // Last Character of Word
        //            if (isBeginRegion && j == linkInfo.linkTextLength - 1)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //            // If Word is split on more than one line.
        //            else if (isBeginRegion && currentLine != text.textInfo.characterInfo[characterIndex + 1].lineNumber)
        //            {
        //                isBeginRegion = false;

        //                br = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.descender, 0));
        //                tr = textTransform.TransformPoint(new Vector3(currentCharInfo.topRight.x, currentCharInfo.ascender, 0));

        //                // Check for Intersection
        //                if (PointIntersectRectangle(position, bl, tl, tr, br))
        //                    return i;

        //                //Debug.Log("End Word Region at [" + currentCharInfo.character + "]");
        //            }
        //        }

        //        // Find the closest line segment to position.
        //        float dbl = DistanceToLine(bl, tl, position);
        //        float dtl = DistanceToLine(tl, tr, position);
        //        float dtr = DistanceToLine(tr, br, position);
        //        float dbr = DistanceToLine(br, bl, position);

        //        float d = dbl < dtl ? dbl : dtl;
        //        d = d < dtr ? d : dtr;
        //        d = d < dbr ? d : dbr;

        //        if (distanceSqr > d)
        //        {
        //            distanceSqr = d;
        //            closest = i;
        //        }
        //        //Debug.Log("Word at Index: " + i + " is located at (" + bl + ", " + tl + ", " + tr + ", " + br + ").");

        //    }
        //    return closest;
        //}



        /// <summary>
        /// Function to check if a Point is contained within a Rectangle.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static bool PointIntersectRectangle(Vector3 m, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            // A zero area rectangle is not valid for this method.
            Vector3 normal = Vector3.Cross(b - a, d - a);
            if (normal == Vector3.zero)
                return false;

            Vector3 ab = b - a;
            Vector3 am = m - a;
            Vector3 bc = c - b;
            Vector3 bm = m - b;

            float abamDot = Vector3.Dot(ab, am);
            float bcbmDot = Vector3.Dot(bc, bm);

            return 0 <= abamDot && abamDot <= Vector3.Dot(ab, ab) && 0 <= bcbmDot && bcbmDot <= Vector3.Dot(bc, bc);
        }


        /// <summary>
        /// Method to convert ScreenPoint to WorldPoint aligned with Rectangle
        /// </summary>
        /// <param name="transform">RectTransform (or Transform) to convert into.</param>
        /// <param name="screenPoint">Screen-space position.</param>
        /// <param name="cam">Camera; null for overlay canvas.</param>
        /// <param name="worldPoint">Resulting world position on the plane defined by the transform.</param>
        /// <returns>Whether a ray from the camera intersects the plane defined by the transform orientation and position.</returns>
        /// <remarks>
        /// Builds a plane using the transform forward and projects the screen ray to recover a world-space point suitable for RectTransform hit tests.
        /// </remarks>
        public static bool ScreenPointToWorldPointInRectangle(Transform transform, Vector2 screenPoint, Camera cam, out Vector3 worldPoint)
        {
            worldPoint = (Vector3)Vector2.zero;
            Ray ray = RectTransformUtility.ScreenPointToRay(cam, screenPoint);

            float enter;
            if (!new Plane(transform.rotation * Vector3.back, transform.position).Raycast(ray, out enter))
                return false;

            worldPoint = ray.GetPoint(enter);

            return true;
        }


        private struct LineSegment
        {
            public Vector3 Point1;
            public Vector3 Point2;

            public LineSegment(Vector3 p1, Vector3 p2)
            {
                Point1 = p1;
                Point2 = p2;
            }
        }


        /// <summary>
        /// Function returning the point of intersection between a line and a plane.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="point"></param>
        /// <param name="normal"></param>
        /// <param name="intersectingPoint"></param>
        /// <returns></returns>
        private static bool IntersectLinePlane(LineSegment line, Vector3 point, Vector3 normal, out Vector3 intersectingPoint)
        {
            intersectingPoint = Vector3.zero;
            Vector3 u = line.Point2 - line.Point1;
            Vector3 w = line.Point1 - point;

            float D = Vector3.Dot(normal, u);
            float N = -Vector3.Dot(normal, w);

            if (Mathf.Abs(D) < Mathf.Epsilon)   // if line is parallel & co-planar to plane
            {
                if (N == 0)
                    return true;
                else
                    return false;
            }

            float sI = N / D;

            if (sI < 0 || sI > 1) // Line parallel to plane
                return false;

            intersectingPoint = line.Point1 + sI * u;

            return true;
        }


        /// <summary>
        /// Method returning the Square Distance from a Point to a Line.
        /// </summary>
        /// <param name="a">First endpoint of the segment in the same space as the test point.</param>
        /// <param name="b">Second endpoint of the segment; when equal to <paramref name="a"/> the method returns squared distance to that point.</param>
        /// <param name="point">Sample position whose closest point on the segment ab is projected to minimize Euclidean distance.</param>
        /// <returns>Squared distance from <paramref name="point"/> to the closest location on segment ab, avoiding a costly square root.</returns>
        /// <remarks>
        /// Used heavily by caret picking to compare proximity to each edge of glyph quads without allocating temporary structs.
        /// </remarks>
        public static float DistanceToLine(Vector3 a, Vector3 b, Vector3 point)
        {
            // If a and b are the same point, just return distance to that point
            if (a == b)
            {
                Vector3 diff = point - a;
                return Vector3.Dot(diff, diff);
            }

            Vector3 n = b - a;
            Vector3 pa = a - point;

            float c = Vector3.Dot( n, pa );

            // Closest point is a
            if ( c > 0.0f )
                return Vector3.Dot( pa, pa );

            Vector3 bp = point - b;

            // Closest point is b
            if (Vector3.Dot( n, bp ) > 0.0f )
                return Vector3.Dot( bp, bp );

            // Closest point is between a and b
            Vector3 e = pa - n * (c / Vector3.Dot( n, n ));

            return Vector3.Dot( e, e );
        }


        /// <summary>
        /// Function returning the Square Distance from a Point to a Line and Direction.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="point"></param>
        /// <param name="direction">-1 left, 0 in between, 1 right</param>
        /// <returns></returns>
        //public static float DistanceToLineDirectional(Vector3 a, Vector3 b, Vector3 point, ref int direction)
        //{
        //    Vector3 n = b - a;
        //    Vector3 pa = a - point;

        //    float c = Vector3.Dot(n, pa);
        //    direction = -1;

        //    // Closest point is a
        //    if (c > 0.0f)
        //        return Vector3.Dot(pa, pa);

        //    Vector3 bp = point - b;
        //    direction = 1;

        //    // Closest point is b
        //    if (Vector3.Dot(n, bp) > 0.0f)
        //        return Vector3.Dot(bp, bp);

        //    // Closest point is between a and b
        //    Vector3 e = pa - n * (c / Vector3.Dot(n, n));

        //    direction = 0;
        //    return Vector3.Dot(e, e);
        //}


        /// <summary>
        /// Table used to convert character to lowercase.
        /// </summary>
        const string k_lookupStringL = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@abcdefghijklmnopqrstuvwxyz[-]^_`abcdefghijklmnopqrstuvwxyz{|}~-";

        /// <summary>
        /// Table used to convert character to uppercase.
        /// </summary>
        const string k_lookupStringU = "-------------------------------- !-#$%&-()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[-]^_`ABCDEFGHIJKLMNOPQRSTUVWXYZ{|}~-";


        /// <summary>
        /// Get lowercase version of this ASCII character.
        /// </summary>
        public static char ToLowerFast(char c)
        {
            if (c > k_lookupStringL.Length - 1)
                return c;

            return k_lookupStringL[c];
        }

        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        public static char ToUpperFast(char c)
        {
            if (c > k_lookupStringU.Length - 1)
                return c;

            return k_lookupStringU[c];
        }

        /// <summary>
        /// Get uppercase version of this ASCII character.
        /// </summary>
        internal static uint ToUpperASCIIFast(uint c)
        {
            if (c > k_lookupStringU.Length - 1)
                return c;

            return k_lookupStringU[(int)c];
        }

        /// <summary>
        /// Returns the case insensitive hashcode for the given string.
        /// </summary>
        /// <param name="s">The string to hash.</param>
        /// <returns>32-bit hash code combining uppercased ASCII letters so differently cased keys collide intentionally.</returns>
        /// <remarks>
        /// Applies the same mixing polynomial used elsewhere in TMP for stable font feature lookups across platforms.
        /// </remarks>
        public static int GetHashCode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0;

            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ ToUpperFast(s[i]);

            return hashCode;
        }

        /// <summary>
        /// Method which returns a simple hashcode from a string.
        /// </summary>
        /// <param name="s">The string to hash.</param>
        /// <returns>Iteratively mixed hash bits using each character code unit without case folding.</returns>
        /// <remarks>
        /// Faster than the case-insensitive variant when the caller already normalized casing.
        /// </remarks>
        public static int GetSimpleHashCode(string s)
        {
            int hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = ((hashCode << 5) + hashCode) ^ s[i];

            return hashCode;
        }

        /// <summary>
        /// Method which returns a simple hashcode from a string converted to lowercase.
        /// </summary>
        /// <param name="s">The string to hash (converted to lowercase).</param>
        /// <returns>Unsigned DJB-style accumulation after folding ASCII characters through the fast lowercase table.</returns>
        /// <remarks>
        /// Matches legacy TMP asset bundle keys that were persisted with lowercase folding for case-insensitive lookups.
        /// </remarks>
        public static uint GetSimpleHashCodeLowercase(string s)
        {
            uint hashCode = 5381;

            for (int i = 0; i < s.Length; i++)
                hashCode = (hashCode << 5) + hashCode ^ ToLowerFast(s[i]);

            return hashCode;
        }

        /// <summary>
        /// Function which returns a simple hash code from a string converted to uppercase.
        /// </summary>
        /// <param name="s">The string from which to compute the hash code.</param>
        /// <returns>The computed hash code.</returns>
        public static uint GetHashCodeCaseInSensitive(string s)
        {
            uint hashCode = 0;

            for (int i = 0; i < s.Length; i++)
                hashCode = (hashCode << 5) + hashCode ^ ToUpperFast(s[i]);

            return hashCode;
        }


        /// <summary>
        /// Method to convert Hex to Int
        /// </summary>
        /// <param name="hex">Single hexadecimal digit character drawn from ASCII ranges 0-9, A-F, or a-f.</param>
        /// <returns>Integer nibble value from 0 through 15, or 15 when the character does not match a known hex digit.</returns>
        /// <remarks>
        /// Fallback to 15 keeps legacy callers from throwing when parsing partially malformed rich-text color attributes.
        /// </remarks>
        public static int HexToInt(char hex)
        {
            switch (hex)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case 'A': return 10;
                case 'B': return 11;
                case 'C': return 12;
                case 'D': return 13;
                case 'E': return 14;
                case 'F': return 15;
                case 'a': return 10;
                case 'b': return 11;
                case 'c': return 12;
                case 'd': return 13;
                case 'e': return 14;
                case 'f': return 15;
            }
            return 15;
        }


        /// <summary>
        /// Method to convert a properly formatted string which contains an hex value to its decimal value.
        /// </summary>
        /// <param name="s">String containing hexadecimal digits without leading prefixes; processed from most significant nibble.</param>
        /// <returns>Combined integer value after multiplying each nibble by successive powers of sixteen.</returns>
        /// <remarks>
        /// Powers characters using <see cref="Mathf.Pow"/> so multi-byte color strings become single ARGB component values.
        /// </remarks>
        public static int StringHexToInt(string s)
        {
            int value = 0;

            for (int i = 0; i < s.Length; i++)
            {
                value += HexToInt(s[i]) * (int)Mathf.Pow(16, (s.Length - 1) - i);
            }

            return value;
        }
    }
}

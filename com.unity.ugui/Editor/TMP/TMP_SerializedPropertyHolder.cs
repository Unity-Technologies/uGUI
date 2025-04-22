using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEditor;
using UnityEngine.TextCore.Text;

namespace TMPro
{
    class TMP_SerializedPropertyHolder : ScriptableObject
    {
        public FontAsset fontAsset;
        public uint firstCharacter;
        public uint secondCharacter;

        public GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = new GlyphPairAdjustmentRecord(new GlyphAdjustmentRecord(), new GlyphAdjustmentRecord());
    }
}

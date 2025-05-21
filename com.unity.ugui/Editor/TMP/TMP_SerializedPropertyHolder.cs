using UnityEngine;
using UnityEngine.TextCore.LowLevel;


namespace TMPro
{
    class TMP_SerializedPropertyHolder : ScriptableObject
    {
        public TMP_FontAsset fontAsset;
        public uint firstCharacter;
        public uint secondCharacter;

        public GlyphPairAdjustmentRecord glyphPairAdjustmentRecord = new GlyphPairAdjustmentRecord(new GlyphAdjustmentRecord(), new GlyphAdjustmentRecord());
    }
}

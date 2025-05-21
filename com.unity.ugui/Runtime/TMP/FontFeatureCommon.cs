

namespace UnityEngine.TextCore
{
    /// <summary>
    /// Enumeration of currently supported OpenType Layout features.
    /// </summary>
    public enum OTL_FeatureTag : uint
    {
        kern = 'k' << 24 | 'e' << 16 | 'r' << 8 | 'n',
        liga = 'l' << 24 | 'i' << 16 | 'g' << 8 | 'a',
        mark = 'm' << 24 | 'a' << 16 | 'r' << 8 | 'k',
        mkmk = 'm' << 24 | 'k' << 16 | 'm' << 8 | 'k',
    }
}

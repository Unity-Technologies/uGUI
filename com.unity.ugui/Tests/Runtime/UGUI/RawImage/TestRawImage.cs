using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Subclass of RawImage to get access to internal/protected properties for tests.
/// </summary>
class TestRawImage : RawImage
{
    public static Texture DefaultWhiteTexture => s_WhiteTexture;
}

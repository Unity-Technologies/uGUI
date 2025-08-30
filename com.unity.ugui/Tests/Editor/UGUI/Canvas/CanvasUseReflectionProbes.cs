using NUnit.Framework;
using UnityEngine;

[Category("Canvas")]
public class CanvasUseReflectionProbes : TestBehaviourBase<UnityEngine.Canvas>
{
    [Test]
    public void OnlyWorldSpaceCanvasCanUseReflectionProbes()
    {
        m_TestObject.useReflectionProbes = true;

        m_TestObject.renderMode = RenderMode.ScreenSpaceOverlay;
        Assert.False(m_TestObject.useReflectionProbes);

        m_TestObject.renderMode = RenderMode.ScreenSpaceCamera;
        Assert.False(m_TestObject.useReflectionProbes);

        m_TestObject.renderMode = RenderMode.WorldSpace;
        Assert.True(m_TestObject.useReflectionProbes);
    }

    [Test]
    public void ProvidesNormals()
    {
        var dummyChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Tangent;

        m_TestObject.renderMode = RenderMode.WorldSpace;
        m_TestObject.additionalShaderChannels = dummyChannels;

        m_TestObject.useReflectionProbes = false;
        Assert.AreEqual(m_TestObject.additionalShaderChannels, dummyChannels);

        m_TestObject.useReflectionProbes = true;
        Assert.AreEqual(m_TestObject.additionalShaderChannels, dummyChannels | AdditionalCanvasShaderChannels.Normal);

        m_TestObject.useReflectionProbes = false;
        Assert.AreEqual(m_TestObject.additionalShaderChannels, dummyChannels);
    }
}

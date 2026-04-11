using NUnit.Framework;
using UnityEngine;
using UnityEditor.EventSystems;
using UnityEngine.EventSystems;

[TestFixture]
internal class InputModuleTests
{
    private EventSystem m_EventSystem;

    [SetUp]
    public void Setup()
    {
        m_EventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_EventSystem.gameObject);
    }

    [Test]
    public void InputModuleComponentFactory_AddComponent_CanBeOverriden()
    {
        // First call creates a StandaloneInputModule
        var inputModule = InputModuleComponentFactory.AddInputModule(m_EventSystem.gameObject);
        Assert.IsInstanceOf<StandaloneInputModule>(inputModule);
        Object.DestroyImmediate(inputModule);

        // After setting the override to a custom type, further calls use the custom type
        InputModuleComponentFactory.SetInputModuleComponentOverride(go => go.AddComponent<TestInputModule>());
        inputModule = InputModuleComponentFactory.AddInputModule(m_EventSystem.gameObject);
        Assert.IsInstanceOf<TestInputModule>(inputModule);
        Object.DestroyImmediate(inputModule);

        // After setting the override to null, further calls use the StandaloneInputModule again
        InputModuleComponentFactory.SetInputModuleComponentOverride(null);
        inputModule = InputModuleComponentFactory.AddInputModule(m_EventSystem.gameObject);
        Assert.IsInstanceOf<StandaloneInputModule>(inputModule);
    }

    internal class TestInputModule : BaseInputModule
    {
        public override void Process() { }
    }
}

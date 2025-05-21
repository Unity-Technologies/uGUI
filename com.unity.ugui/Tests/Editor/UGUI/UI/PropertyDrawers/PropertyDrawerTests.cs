using System;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Search;

[Timeout(360000)]
public class PropertyDrawerTests
{
    class PropertyDrawerTestsWindow : EditorWindow
    {
        public Navigation navigation;
        public SpriteState spriteState;
        public Dropdown.OptionDataList optionDataList;
        [SearchContext("t:gameObject is:gameObject", "gameObject")]
        public GameObject searchContext;

        SerializedObject serializedObject;

        void CreateGUI()
        {
            serializedObject = new SerializedObject(this);

            Add(nameof(navigation));
            Add(nameof(spriteState));
            Add(nameof(optionDataList));
            Add(nameof(searchContext));

            rootVisualElement.Bind(serializedObject);

            // Forces visual tree update
            Rebuild();
        }

        void Add(string propertyName)
        {
            rootVisualElement.Add(new PropertyField() { bindingPath = propertyName });
        }

        public SerializedProperty Property(string propertyName) => serializedObject.FindProperty(propertyName);

        public void Rebuild() => rootVisualElement.Bind(serializedObject);
    }

    static PropertyDrawerTestsWindow window;

    [UnitySetUp]
    [MenuItem("Tests/Open Property Drawer Test Window")]
    public static IEnumerator SetUp()
    {
        VisualTreeBindingsUpdater.disableBindingsThrottling = true;
        window = EditorWindow.GetWindow<PropertyDrawerTestsWindow>();
        window.Repaint();
        window.Show();
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        VisualTreeBindingsUpdater.disableBindingsThrottling = false;
        window.Close();
    }

    [UnityTest]
    public IEnumerator NavigationDrawer_IsVisible()
    {
        yield return null;
        Assert.IsNotNull(window.rootVisualElement.Query<VisualElement>("Navigation").Build().First());
    }

    [UnityTest]
    public IEnumerator SpriteStateDrawer_IsVisible()
    {
        yield return null;
        Assert.IsNotNull(window.rootVisualElement.Q("SpriteState"));
    }

    [UnityTest]
    [Ignore("UUM-18482")]
    public IEnumerator DropdownOptionDataListDrawer_IsVisible()
    {
        yield return null;
        Assert.IsNotNull(window.rootVisualElement.Q("DropdownOptionDataList"), $"Item is null. Root object count: {window.rootVisualElement.childCount}");
    }

    [UnityTest]
    public IEnumerator SearchContextDrawer_IsVisible()
    {
        yield return null;
        Assert.IsNotNull(window.rootVisualElement.Q("SearchContext"));
    }

    // Fake expected result in order to make TestCaseAttribute to work with UnityTest
    [UnityTest]
    [TestCase(new object[] { Navigation.Mode.None, 0}, ExpectedResult = null)]
    [TestCase(new object[] { Navigation.Mode.Horizontal, 1 }, ExpectedResult = null)]
    [TestCase(new object[] { Navigation.Mode.Vertical, 1 }, ExpectedResult = null)]
    [TestCase(new object[] { Navigation.Mode.Automatic, 0 }, ExpectedResult = null)]
    [TestCase(new object[] { Navigation.Mode.Explicit, 4 }, ExpectedResult = null)]
    [TestCase(new object[] { (Navigation.Mode.Explicit | Navigation.Mode.Horizontal), 0 }, ExpectedResult = null)]
    [TestCase(new object[] { (Navigation.Mode.Automatic | Navigation.Mode.Explicit), 0 }, ExpectedResult = null)]
    public static IEnumerator NavigationDrawer_ShowsCorrectAdditionalControlCount(Enum mode, int expectedCount)
    {
        var field = window.rootVisualElement.Q<EnumFlagsField>("unity-input-navigation.m_Mode");
        field.value = mode;
        yield return null;

        var indent = window.rootVisualElement.Q<VisualElement>("Navigation").Q<VisualElement>("Indent");
        var visibleChildren = indent.Children().Count(child => child.resolvedStyle.display != DisplayStyle.None);
        Assert.AreEqual(expectedCount, visibleChildren, $"{expectedCount} additional Navigation object properties should be " +
            $"visible when 'Mode' is set to '{(Navigation.Mode)window.Property("navigation.m_Mode").enumValueFlag}'");
    }
}

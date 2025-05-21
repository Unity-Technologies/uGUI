using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace TMPro
{
    public class TMP_ControlTests
    {
        Scene scene;
        [SetUp]
        public void Setup()
        {
            // Create a new scene and open it
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        [TestCase("GameObject/UI/Dropdown - TextMeshPro")]
        [TestCase("GameObject/UI/Button - TextMeshPro")]
        [TestCase("GameObject/UI/Input Field - TextMeshPro")]
        [TestCase("GameObject/UI/Text - TextMeshPro")]
        public void TMPControlCreationAndUndoTest(string menuItem)
        {
            Assert.AreEqual(0, scene.rootCount);

            EditorApplication.ExecuteMenuItem(menuItem);

            // After creating a TMP control, objects in the scene should be Canvas, EventSystem, and the TMP control
            Assert.AreEqual(2, scene.rootCount);

            Undo.PerformUndo();

            // After undoing, the scene should be back to its original state
            Assert.AreEqual(0, scene.rootCount);
        }
    }
}

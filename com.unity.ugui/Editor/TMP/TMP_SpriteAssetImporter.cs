using UnityEngine;
using UnityEditor;
using UnityEditor.TextCore.Text;


namespace TMPro
{
    internal class TMP_SpriteAssetImporter : EditorWindow
    {
        // Create Sprite Asset Editor Window
        //[MenuItem("Window/TextMeshPro/Sprite Importer", false, 2026)]
        public static void ShowSpriteImporterWindow()
        {
            var window = GetWindow<SpriteAssetImporter>();
            window.titleContent = new GUIContent("Sprite Importer");
            window.Focus();
        }
    }
}

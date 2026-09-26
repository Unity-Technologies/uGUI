using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
    [FilePath("ProjectSettings/TextMeshProSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    class TMP_EditorProjectSettings : ScriptableSingleton<TMP_EditorProjectSettings>
    {
        [SerializeField]
        bool m_IncludeBuiltInSettingsInBuilds = true;

        /// <summary>
        /// Whether player builds include the built-in TMP Settings, and with them the default font asset and shaders,
        /// when the project has no TMP Settings asset of its own. Turn off for projects that do not use TextMesh Pro.
        /// </summary>
        public bool includeBuiltInSettingsInBuilds
        {
            get => m_IncludeBuiltInSettingsInBuilds;
            set
            {
                if (m_IncludeBuiltInSettingsInBuilds == value)
                    return;
                m_IncludeBuiltInSettingsInBuilds = value;
                Save(true);
            }
        }
    }
}

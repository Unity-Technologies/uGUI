using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnityEditor.UI")]
[assembly: InternalsVisibleTo("Unity.TextMeshPro")]
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("PlaymodeTests")]
[assembly: InternalsVisibleTo("UnityEditor.UI.EditorTests")]
[assembly: InternalsVisibleTo("UnityEngine.UI.Tests")]
[assembly: InternalsVisibleTo("Unity.CrossModule.UIElementsUGUI.Tests.Runtime")]
#endif

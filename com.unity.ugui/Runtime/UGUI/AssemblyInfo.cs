using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnityEditor.UI")]
[assembly: InternalsVisibleTo("Unity.TextMeshPro")]
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("PlaymodeTests")]
[assembly: InternalsVisibleTo("UnityEditor.UI.EditorTests")]
#endif

using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnityEditor.UI")]
#if UNITY_INCLUDE_TESTS
[assembly: InternalsVisibleTo("PlaymodeTests")]
#endif

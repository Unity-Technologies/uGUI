using UnityEngine;
namespace TMPro
{
    /// <summary>
    /// HelpURLAttribute wrapper for TMP C# code
    /// </summary>
    internal class TMPHelpURL : UIModuleHelpURL
    {
        internal TMPHelpURL(string pageName)
            : base($"TextMeshPro/{pageName}") { }
    }
}

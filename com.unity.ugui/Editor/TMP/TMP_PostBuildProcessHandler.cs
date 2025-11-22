using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System;


namespace TMPro
{
    public class TMP_PostBuildProcessHandler
    {
        [PostProcessBuildAttribute(10000)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target == BuildTarget.iOS)
            {
                // Try loading the TMP Settings
                TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");

                if (settings == null || TMP_Settings.enableEmojiSupport == false)
                    return;
                
                string file = "";
                
                switch (PlayerSettings.xcodeProjectType)
                {
                    case XcodeProjectType.ObjectiveC:
                        file = Path.Combine(pathToBuiltProject, "Trampoline", "Classes/UI/Keyboard.mm");
                        if (!File.Exists(file))
                        {
                            file = Path.Combine(pathToBuiltProject, "Classes/UI/Keyboard.mm");
                        }
                        break;
                    case XcodeProjectType.Swift:
                        file = Path.Combine(pathToBuiltProject, "UnityFramework/Features/Keyboard/Keyboard.mm");
                        break;
                    default:
                        throw new Exception("Unsupported iOS Xcode project type. Will not be able to modify Keyboard.mm to disable emoji filtering.");
                }

                if (!File.Exists(file))
                {
                    throw new Exception("Could not enable emojis support. Failed to locate Keyboard.mm file.");
                }
                
                string content = File.ReadAllText(file);
                content = content.Replace("FILTER_EMOJIS_IOS_KEYBOARD 1", "FILTER_EMOJIS_IOS_KEYBOARD 0");
                File.WriteAllText(file, content);
            }
        }
    }
}

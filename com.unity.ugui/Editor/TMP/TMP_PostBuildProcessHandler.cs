#if UNITY_IOS
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System;
using System.IO;


namespace TMPro
{
    public class TMP_PostBuildProcessHandler
    {
        const string kEmojiFilterCDefine = "FILTER_EMOJIS_IOS_KEYBOARD=0";
        // Swift -D flags carry no value, so the Swift trampoline uses a presence-only flag.
        const string kEmojiFilterSwiftFlag = "-DFILTER_EMOJIS_IOS_KEYBOARD_DISABLED";

        [PostProcessBuildAttribute(10000)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS)
                return;

            string flag;
            switch (PlayerSettings.xcodeProjectType)
            {
                case XcodeProjectType.ObjectiveC:
                    flag = kEmojiFilterCDefine;
                    break;
                case XcodeProjectType.Swift:
                    flag = kEmojiFilterSwiftFlag;
                    break;
                default:
                    throw new Exception("Unsupported iOS Xcode project type. Will not be able to configure emoji filtering.");
            }

            // Try loading the TMP Settings
            var settings = Resources.Load<TMP_Settings>("TMP Settings");
            var enableEmojis = settings != null && TMP_Settings.enableEmojiSupport;

            var pbxProjectPath = FindUnityPbxProjectPath(pathToBuiltProject);
            if (pbxProjectPath == null)
            {
                // With emoji support disabled there is nothing to configure; stay inert.
                if (!enableEmojis)
                    return;
                throw new Exception("Could not enable emoji support. Failed to locate the Unity Xcode project.");
            }

            // Skip parsing and rewriting the project when it is already in the desired state.
            var flagPresent = File.ReadAllText(pbxProjectPath).Contains(flag);
            if (flagPresent == enableEmojis)
                return;

            var project = new PBXProject();
            project.ReadFromFile(pbxProjectPath);

            switch (PlayerSettings.xcodeProjectType)
            {
                case XcodeProjectType.ObjectiveC:
                {
                    var targetGuid = new[] { project.GetUnityFrameworkTargetGuid() };
                    if (enableEmojis)
                        project.UpdateBuildProperty(targetGuid, "GCC_PREPROCESSOR_DEFINITIONS", new[] { "$(inherited)", kEmojiFilterCDefine }, new string[] {});
                    else
                        project.UpdateBuildProperty(targetGuid, "GCC_PREPROCESSOR_DEFINITIONS", new string[] {}, new[] { kEmojiFilterCDefine });
                    break;
                }
                case XcodeProjectType.Swift:
                {
                    var targetGuid = new[] { project.TargetGuidByName("UnityAPI") };
                    if (string.IsNullOrEmpty(targetGuid[0]))
                        throw new Exception("Could not configure emoji support. Failed to locate the UnityAPI target.");

                    if (enableEmojis)
                        project.UpdateBuildProperty(targetGuid, "OTHER_SWIFT_FLAGS", new[] { kEmojiFilterSwiftFlag }, new string[] {});
                    else
                        project.UpdateBuildProperty(targetGuid, "OTHER_SWIFT_FLAGS", new string[] {}, new[] { kEmojiFilterSwiftFlag });
                    break;
                }
            }

            project.WriteToFile(pbxProjectPath);
        }

        static string FindUnityPbxProjectPath(string pathToBuiltProject)
        {
            try
            {
                // Locates the Unity-generated project for both Xcode project types regardless of the project name.
                return PBXProject.GetPBXProjectPath(pathToBuiltProject);
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
}
#endif

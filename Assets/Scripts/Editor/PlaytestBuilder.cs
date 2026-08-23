using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace RentIsDue.Editor
{
    public class PlaytestBuilder
    {
        [MenuItem("Tools/Build Windows Playtest (.exe)")]
        public static void BuildWindowsExe()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            string buildFolder = Path.Combine(projectRoot, "Builds", "RentIsDue_Playtest");
            
            if (!Directory.Exists(buildFolder))
            {
                Directory.CreateDirectory(buildFolder);
            }

            string exePath = Path.Combine(buildFolder, "RentIsDue.exe");

            // Lấy danh sách Scene đang kích hoạt trong Build Settings
            string[] scenes = { "Assets/Scenes/SampleScene.unity" };

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[PlaytestBuilder] Building game to: {exePath}...");
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"<color=green>[PlaytestBuilder] Build succeeded! Total size: {summary.totalSize / 1024 / 1024:F1} MB</color>");
                EditorUtility.DisplayDialog("Build Succeeded", $"Playtest build created successfully!\n\nLocation: {buildFolder}", "Open Folder");
                EditorUtility.RevealInFinder(exePath);
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[PlaytestBuilder] Build failed with {summary.totalErrors} error(s).");
                EditorUtility.DisplayDialog("Build Failed", "Build failed. Check Console for details.", "OK");
            }
        }
    }
}

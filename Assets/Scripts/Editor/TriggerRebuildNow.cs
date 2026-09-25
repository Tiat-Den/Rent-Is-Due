using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace RentIsDue.Editor
{
    [InitializeOnLoad]
    public static class TriggerRebuildNow
    {
        static TriggerRebuildNow()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;

                Debug.Log("<color=cyan>[TriggerRebuildNow] Starting automatic scene rebuild and NPC unpacking...</color>");
                RoomSceneBuilder.BuildGiantRoom();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=green>[TriggerRebuildNow] Rebuild complete and scene saved cleanly!</color>");

                // Safely delete this one-off trigger script
                string scriptPath = "Assets/Scripts/Editor/TriggerRebuildNow.cs";
                AssetDatabase.DeleteAsset(scriptPath);
            };
        }
    }
}

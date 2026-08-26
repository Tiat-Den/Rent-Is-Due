using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SetupBuildScenes
{
    [MenuItem("Tools/📦 Thiết lập Game (Mốc Alpha)")]
    public static void SetupScenes()
    {
        string scenesDir = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesDir))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string roomScenePath = scenesDir + "/RoomScene.unity";
        string mainMenuPath = scenesDir + "/MainMenu.unity";

        // 1. Tạo MainMenu Scene nếu chưa có
        if (!System.IO.File.Exists(mainMenuPath))
        {
            UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            
            // Thêm Camera
            GameObject cam = new GameObject("Main Camera");
            var c = cam.AddComponent<Camera>();
            c.backgroundColor = Color.black;
            c.clearFlags = CameraClearFlags.SolidColor;

            // Thêm MainMenuManager
            GameObject manager = new GameObject("MainMenuManager");
            manager.AddComponent<RentIsDue.Core.MainMenuManager>();

            EditorSceneManager.SaveScene(newScene, mainMenuPath);
            Debug.Log("Created MainMenu scene at " + mainMenuPath);
        }

        // 2. Thêm vào Build Settings (Build Profiles trong Unity 6)
        List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
        
        buildScenes.Add(new EditorBuildSettingsScene(mainMenuPath, true));
        if (System.IO.File.Exists(roomScenePath))
        {
            buildScenes.Add(new EditorBuildSettingsScene(roomScenePath, true));
        }

        EditorBuildSettings.scenes = buildScenes.ToArray();
        Debug.Log("<color=green>Đã thiết lập xong Scenes vào Build Settings! Bạn có thể ấn nút Play từ MainMenu để chơi.</color>");
        
        // Mở sẵn Main Menu cho user
        EditorSceneManager.OpenScene(mainMenuPath);
    }
}

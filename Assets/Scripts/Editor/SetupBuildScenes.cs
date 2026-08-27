using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SetupBuildScenes
{
    [MenuItem("Tools/?? Thi?t l?p Game (M?c Alpha)")]
    public static void SetupScenes()
    {
        string scenesDir = "Assets/Scenes";
        if (!AssetDatabase.IsValidFolder(scenesDir))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        string roomScenePath = scenesDir + "/SampleScene.unity";
        string mainMenuPath = scenesDir + "/MainMenu.unity";

        // 1. T?o MainMenu Scene n?u chua có
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
        Debug.Log("<color=green>Ðã thi?t l?p xong Scenes vào Build Settings! B?n có th? ?n nút Play t? MainMenu d? choi.</color>");
        
        // M? s?n Main Menu cho user
        EditorSceneManager.OpenScene(mainMenuPath);
    }
}

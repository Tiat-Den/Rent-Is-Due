using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public bool isPaused { get; private set; } = false;
    private bool isSettingsOpen = false;

    void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isSettingsOpen)
            {
                isSettingsOpen = false;
                return;
            }

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;
        }
    }

    void OnGUI()
    {
        if (!isPaused) return;

        float width = 240;
        float height = 280;
        float x = (Screen.width - width) / 2f;
        float y = (Screen.height - height) / 2f;

        if (isSettingsOpen)
        {
            GUILayout.BeginArea(new Rect(x, y, width, height), "Settings", GUI.skin.window);
            
            GUILayout.Space(25);

            RentIsDue.Player.CameraController cam = FindAnyObjectByType<RentIsDue.Player.CameraController>();
            float currentSens = cam != null ? cam.mouseSensitivity : PlayerPrefs.GetFloat("MouseSensitivity", 0.8f);

            GUILayout.Label($"Mouse Sensitivity: {currentSens:F2}");
            
            float newSens = GUILayout.HorizontalSlider(currentSens, 0.1f, 3.0f);
            if (Mathf.Abs(newSens - currentSens) > 0.001f)
            {
                if (cam != null)
                {
                    cam.mouseSensitivity = newSens;
                }
                PlayerPrefs.SetFloat("MouseSensitivity", newSens);
                PlayerPrefs.Save();
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Reset Default"))
            {
                float defaultSens = 0.8f;
                if (cam != null) cam.mouseSensitivity = defaultSens;
                PlayerPrefs.SetFloat("MouseSensitivity", defaultSens);
                
                AudioListener.volume = 1f;
                PlayerPrefs.SetFloat("MasterVolume", 1f);
                
                PlayerPrefs.Save();
            }

            GUILayout.Space(15);
            
            float currentVol = AudioListener.volume;
            GUILayout.Label($"Master Volume: {currentVol * 100f:F0}%");
            float newVol = GUILayout.HorizontalSlider(currentVol, 0f, 1f);
            if (Mathf.Abs(newVol - currentVol) > 0.001f)
            {
                AudioListener.volume = newVol;
                PlayerPrefs.SetFloat("MasterVolume", newVol);
                PlayerPrefs.Save();
            }

            GUILayout.Space(15);

            if (GUILayout.Button("Back"))
            {
                isSettingsOpen = false;
            }

            GUILayout.EndArea();
        }
        else
        {
            GUILayout.BeginArea(new Rect(x, y, width, height), "Pause Menu", GUI.skin.window);
            
            GUILayout.Space(20);

            if (GUILayout.Button("Resume"))
            {
                isPaused = false;
                Time.timeScale = 1f;
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Save Game"))
            {
                if (RentIsDue.Core.SaveManager.Instance != null)
                {
                    RentIsDue.Core.SaveManager.Instance.SaveGame();
                }
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Load Game"))
            {
                if (RentIsDue.Core.SaveManager.Instance != null)
                {
                    RentIsDue.Core.SaveManager.Instance.LoadGame();
                    isPaused = false;
                    Time.timeScale = 1f;
                }
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Settings"))
            {
                isSettingsOpen = true;
            }

            GUILayout.Space(8);

            if (GUILayout.Button("Quit"))
            {
                Debug.Log("Quitting to Main Menu...");
                isPaused = false;
                Time.timeScale = 1f;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }
    }
}

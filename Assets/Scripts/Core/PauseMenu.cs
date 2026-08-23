using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public bool isPaused { get; private set; } = false;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
    }

    void OnGUI()
    {
        if (isPaused)
        {
            float width = 200;
            float height = 250;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            GUILayout.BeginArea(new Rect(x, y, width, height), "Pause Menu", GUI.skin.window);
            
            GUILayout.Space(20);

            if (GUILayout.Button("Resume"))
            {
                isPaused = false;
                Time.timeScale = 1f;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Save Game"))
            {
                if (RentIsDue.Core.SaveManager.Instance != null)
                {
                    RentIsDue.Core.SaveManager.Instance.SaveGame();
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Load Game"))
            {
                if (RentIsDue.Core.SaveManager.Instance != null)
                {
                    RentIsDue.Core.SaveManager.Instance.LoadGame();
                    isPaused = false;
                    Time.timeScale = 1f;
                }
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Quit"))
            {
                Debug.Log("Quitting...");
                Application.Quit();
            }

            GUILayout.EndArea();
        }
    }
}

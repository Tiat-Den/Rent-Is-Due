using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    private bool isPaused = false;

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
            float height = 150;
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

            if (GUILayout.Button("Quit"))
            {
                Debug.Log("Quitting...");
                Application.Quit();
            }

            GUILayout.EndArea();
        }
    }
}

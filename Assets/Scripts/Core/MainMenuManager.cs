using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace RentIsDue.Core
{
    public class MainMenuManager : MonoBehaviour
    {
        private void OnGUI()
        {
            float w = 300f;
            float h = 250f;
            Rect rect = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - h / 2f, w, h);
            
            GUI.Window(0, rect, DrawMenu, "RENT IS DUE");
        }

        private void DrawMenu(int id)
        {
            GUI.DrawTexture(new Rect(0, 0, 300, 250), Texture2D.blackTexture);
            
            GUILayout.Space(20);
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.yellow;
            GUILayout.Label("RENT IS DUE", titleStyle);
            GUILayout.Space(40);

            string savePath = Application.persistentDataPath + "/save.json";
            bool hasSave = File.Exists(savePath);

            GUI.enabled = hasSave;
            if (GUILayout.Button("TIẾP TỤC (Continue)", GUILayout.Height(40)))
            {
                SceneManager.LoadScene("RoomScene");
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            
            if (GUILayout.Button("CHƠI MỚI (New Game)", GUILayout.Height(40)))
            {
                if (hasSave)
                {
                    File.Delete(savePath);
                }
                SceneManager.LoadScene("RoomScene");
            }

            GUILayout.Space(10);
            if (GUILayout.Button("THOÁT GAME (Quit)", GUILayout.Height(40)))
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
    }
}

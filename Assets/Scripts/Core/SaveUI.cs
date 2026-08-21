using UnityEngine;

namespace RentIsDue.Core
{
    public class SaveUI : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, Screen.height - 90, 150, 80));
            
            if (GUILayout.Button("Save Game", GUILayout.Height(35)))
            {
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.SaveGame();
                }
            }
            
            if (GUILayout.Button("Load Game", GUILayout.Height(35)))
            {
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.LoadGame();
                }
            }
            
            GUILayout.EndArea();
        }
    }
}

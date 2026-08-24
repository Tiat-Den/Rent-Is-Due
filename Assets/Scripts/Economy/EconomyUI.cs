using UnityEngine;

namespace RentIsDue.Economy
{
    public class EconomyUI : MonoBehaviour
    {
        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            if (EconomyManager.Instance != null)
            {
                float width = 150f;
                float height = 30f;
                Rect rect = new Rect(Screen.width - width - 10f, 10f, width, height);
                
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 18;
                style.alignment = TextAnchor.MiddleCenter;

                GUI.Box(rect, $"Money: ${EconomyManager.Instance.currentMoney:F2}", style);
            }
        }
    }
}

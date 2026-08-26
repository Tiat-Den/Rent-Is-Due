using System.Collections;
using UnityEngine;

namespace RentIsDue.Gameplay
{
    public class MorningEventUI : MonoBehaviour
    {
        public static MorningEventUI Instance { get; private set; }

        private bool _isShowing = false;
        private string _title = "";
        private string _desc = "";
        private Color _color = Color.white;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ShowEvent(string title, string desc, Color color)
        {
            _title = title;
            _desc = desc;
            _color = color;
            _isShowing = true;
            
            StopAllCoroutines();
            StartCoroutine(HideAfterSeconds(6f)); // Hiển thị trong 6 giây
        }

        private IEnumerator HideAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _isShowing = false;
        }

        private void OnGUI()
        {
            if (!_isShowing) return;

            float w = 500f, h = 100f;
            float x = (Screen.width - w) / 2f;
            float y = 80f; // Hiển thị ở nửa trên màn hình

            // Vẽ nền đen mờ
            Color oldColor = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);
            
            // Vẽ Viền
            GUI.color = _color;
            GUI.DrawTexture(new Rect(x, y, w, 4), Texture2D.whiteTexture); // Top
            GUI.DrawTexture(new Rect(x, y + h - 4, w, 4), Texture2D.whiteTexture); // Bottom

            // Vẽ chữ
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = _color;

            GUIStyle descStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14
            };
            descStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(x, y + 15, w, 30), _title, titleStyle);
            GUI.Label(new Rect(x + 20, y + 50, w - 40, 40), _desc, descStyle);

            GUI.color = oldColor;
        }
    }
}

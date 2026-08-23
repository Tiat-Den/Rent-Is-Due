using System.Collections.Generic;
using UnityEngine;

namespace RentIsDue.Core
{
    public class FloatingFeedbackUI : MonoBehaviour
    {
        public static FloatingFeedbackUI Instance { get; private set; }

        private class Notification
        {
            public string text;
            public Color color;
            public float timer;
            public float duration;
            public float yOffset;
        }

        private List<Notification> activeNotifications = new List<Notification>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void ShowMessage(string message, Color color, float duration = 2.5f)
        {
            activeNotifications.Add(new Notification
            {
                text = message,
                color = color,
                timer = duration,
                duration = duration,
                yOffset = 0f
            });
        }

        private void Update()
        {
            for (int i = activeNotifications.Count - 1; i >= 0; i--)
            {
                var notif = activeNotifications[i];
                notif.timer -= Time.deltaTime;
                notif.yOffset += Time.deltaTime * 25f; // Trôi nhẹ lên trên

                if (notif.timer <= 0f)
                {
                    activeNotifications.RemoveAt(i);
                }
            }
        }

        private void OnGUI()
        {
            if (activeNotifications.Count == 0) return;

            float centerX = Screen.width / 2f;
            float startY = Screen.height * 0.65f;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;

            for (int i = 0; i < activeNotifications.Count; i++)
            {
                var notif = activeNotifications[i];
                float alpha = Mathf.Clamp01(notif.timer / (notif.duration * 0.4f));
                
                Color c = notif.color;
                c.a = alpha;
                style.normal.textColor = c;

                float y = startY - notif.yOffset - (i * 28f);
                GUI.Label(new Rect(centerX - 250, y, 500, 30), notif.text, style);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Economy;

namespace RentIsDue.Core
{
    public class ObjectiveHUD : MonoBehaviour
    {
        public static ObjectiveHUD Instance { get; private set; }

        private bool showControlsHelp = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            // Nhấn H để ẩn/hiện bảng phím tắt trợ giúp
            if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            {
                showControlsHelp = !showControlsHelp;
            }
        }

        private void OnGUI()
        {
            if (Time.timeScale == 0f) return;

            // 1. Khung Mục tiêu Ngày (Objective Box) ở góc trên bên trái
            float currentMoney = EconomyManager.Instance != null ? EconomyManager.Instance.currentMoney : 0f;
            int currentRent = DayManager.Instance != null ? DayManager.Instance.currentRent : 100;
            int day = DayManager.Instance != null ? DayManager.Instance.currentDay : 1;

            float progress = Mathf.Clamp01(currentMoney / Mathf.Max(1, currentRent));

            GUILayout.BeginArea(new Rect(15, 15, 260, 95), GUI.skin.box);
            GUILayout.Label($"<b>🎯 DAY {day} GOAL: PAY RENT</b>");
            
            string moneyColor = currentMoney >= currentRent ? "green" : "yellow";
            GUILayout.Label($"Rent Target: <b><color={moneyColor}>${currentMoney:F1} / ${currentRent}</color></b> ({(int)(progress * 100)}%)");
            
            // Vẽ thanh tiến trình kiếm tiền
            Rect progressRect = GUILayoutUtility.GetRect(240, 10);
            GUI.Box(progressRect, "");
            Color orig = GUI.color;
            GUI.color = currentMoney >= currentRent ? Color.green : new Color(1f, 0.8f, 0.2f);
            GUI.DrawTexture(new Rect(progressRect.x, progressRect.y, progressRect.width * progress, progressRect.height), Texture2D.whiteTexture);
            GUI.color = orig;

            if (currentMoney >= currentRent)
            {
                GUILayout.Label("<color=green>✓ Ready to pay at 22:00!</color>");
            }
            else
            {
                GUILayout.Label($"<color=white>Need <b>${(currentRent - currentMoney):F1}</b> more before 22:00</color>");
            }
            GUILayout.EndArea();

            // 2. Bảng Phím tắt Trợ giúp (Controls Helper) ở góc dưới bên phải
            if (showControlsHelp)
            {
                float helpWidth = 220;
                float helpHeight = 145;
                float hx = Screen.width - helpWidth - 15;
                float hy = Screen.height - helpHeight - 15;

                GUILayout.BeginArea(new Rect(hx, hy, helpWidth, helpHeight), "Controls (H to Hide)", GUI.skin.window);
                GUILayout.Label("• <b>WASD</b>: Move");
                GUILayout.Label("• <b>Shift</b>: Sprint (Run faster)");
                GUILayout.Label("• <b>Space</b>: Jump");
                GUILayout.Label("• <b>E</b>: Interact / Search");
                GUILayout.Label("• <b>Tab</b>: Toggle Inventory");
                GUILayout.Label("• <b>ESC</b>: Pause / Save / Settings");
                GUILayout.EndArea();
            }
        }
    }
}

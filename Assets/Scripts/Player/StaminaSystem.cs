using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Shop;

namespace RentIsDue.Player
{
    public class StaminaSystem : MonoBehaviour
    {
        public static StaminaSystem Instance { get; private set; }

        [Header("Stamina Settings")]
        public float currentStamina = 100f;
        public float regenRate = 25f; // hồi 25 thể lực / giây
        public float sprintDrainRate = 22f; // tiêu tốn khi chạy nhanh
        public float jumpCost = 12f;

        public bool isSprinting { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            currentStamina = GetMaxStamina();
        }

        public float GetMaxStamina()
        {
            if (UpgradeManager.Instance != null)
            {
                return UpgradeManager.Instance.GetMaxStamina();
            }
            return 100f;
        }

        private void Update()
        {
            float maxStamina = GetMaxStamina();

            // Kiểm tra phím Shift để Sprint
            bool wantsToSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
            PlayerMovement pm = GetComponent<PlayerMovement>();

            // Chỉ tính sprint khi đang di chuyển và còn thể lực
            if (wantsToSprint && currentStamina > 3f)
            {
                isSprinting = true;
                currentStamina = Mathf.Max(0f, currentStamina - sprintDrainRate * Time.deltaTime);
            }
            else
            {
                isSprinting = false;
                // Hồi phục thể lực
                if (currentStamina < maxStamina)
                {
                    currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
                }
            }
        }

        public bool CanJump()
        {
            return currentStamina >= jumpCost;
        }

        public void ConsumeJumpStamina()
        {
            currentStamina = Mathf.Max(0f, currentStamina - jumpCost);
        }

        private void OnGUI()
        {
            if (Time.timeScale == 0f) return;

            float maxStamina = GetMaxStamina();
            float staminaPercent = Mathf.Clamp01(currentStamina / maxStamina);

            // Vẽ thanh Stamina ở góc dưới giữa màn hình
            float barWidth = 220f;
            float barHeight = 12f;
            float x = (Screen.width - barWidth) / 2f;
            float y = Screen.height - 45f;

            // Khung nền đen
            GUI.Box(new Rect(x - 2, y - 2, barWidth + 4, barHeight + 4), "");
            
            // Thanh tiến trình thể lực (màu xanh lá / vàng khi gần hết)
            Color barColor = staminaPercent > 0.3f ? new Color(0.2f, 0.85f, 0.3f) : new Color(0.95f, 0.6f, 0.1f);
            Color orig = GUI.color;
            GUI.color = barColor;
            GUI.DrawTexture(new Rect(x, y, barWidth * staminaPercent, barHeight), Texture2D.whiteTexture);
            GUI.color = orig;

            // Nhãn thể lực
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 11;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y - 18, barWidth, 20), $"⚡ STAMINA: {(int)currentStamina} / {(int)maxStamina}", style);
        }
    }
}

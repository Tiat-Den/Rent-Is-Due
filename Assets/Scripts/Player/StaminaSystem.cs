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
        [Tooltip("Tốc độ hồi phục thể lực cơ bản mỗi giây")]
        public float baseRegenRate = 4.0f; // cơ bản 4.0/s
        [Tooltip("Thời gian chờ sau khi chạy/nhảy trước khi bắt đầu hồi phục")]
        public float regenDelay = 1.2f; // nghỉ 1.2s mới bắt đầu hồi
        public float sprintDrainRate = 22f; // tiêu tốn khi chạy nhanh
        public float jumpCost = 15f;

        public bool isSprinting { get; private set; } = false;
        private float lastStaminaConsumeTime = -999f;

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

        public float GetRegenRate()
        {
            if (UpgradeManager.Instance != null)
            {
                return UpgradeManager.Instance.GetStaminaRegenRate();
            }
            return baseRegenRate;
        }

        private void Update()
        {
            float maxStamina = GetMaxStamina();
            float currentRegenRate = GetRegenRate();

            // Kiểm tra phím Shift để Sprint
            bool wantsToSprint = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;

            // Chỉ tính sprint khi đang di chuyển và còn thể lực
            if (wantsToSprint && currentStamina > 3f)
            {
                isSprinting = true;
                currentStamina = Mathf.Max(0f, currentStamina - sprintDrainRate * Time.deltaTime);
                lastStaminaConsumeTime = Time.time;
            }
            else
            {
                isSprinting = false;
                // Chỉ hồi phục khi đã nghỉ đủ thời gian regenDelay (1.2 giây)
                if (Time.time >= lastStaminaConsumeTime + regenDelay)
                {
                    if (currentStamina < maxStamina)
                    {
                        currentStamina = Mathf.Min(maxStamina, currentStamina + currentRegenRate * Time.deltaTime);
                    }
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
            lastStaminaConsumeTime = Time.time;
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

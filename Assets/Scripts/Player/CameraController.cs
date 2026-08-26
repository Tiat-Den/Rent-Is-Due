using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Core;

namespace RentIsDue.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Positioning")]
        public Transform playerBody;
        public Vector3 eyeOffset = new Vector3(0, 2.2f, 0); // Tầm nhìn cao thoáng (2.2m) dễ quan sát phòng và đồ đạc

        [Header("Mouse Look Settings")]
        [Range(0.1f, 3f)]
        public float mouseSensitivity = 0.8f;
        public float minPitch = -85f;
        public float maxPitch = 85f;

        private float xRotation = 0f;

        private PauseMenu pauseMenu;
        private Inventory.InventoryUI inventoryUI;
        private Shop.UpgradeUI upgradeUI;

        private void Start()
        {
            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", mouseSensitivity);
            FindPlayer();

            pauseMenu = FindAnyObjectByType<PauseMenu>();
            inventoryUI = FindAnyObjectByType<Inventory.InventoryUI>();
            upgradeUI = FindAnyObjectByType<Shop.UpgradeUI>();

            // Đặt góc nhìn rộng FPS chuẩn 80 độ (Human Natural Perspective) chống zoom ảo
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = 80f;
            }

            SetCursorState(true);
        }

        private void FindPlayer()
        {
            if (playerBody == null)
            {
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                if (player != null)
                {
                    playerBody = player.transform;
                }
            }
        }

        private bool IsAnyUIOpen()
        {
            if (Time.timeScale == 0f) return true;
            if (DaySummaryUI.Instance != null && DaySummaryUI.Instance.isShowingSummary) return true;
            if (pauseMenu != null && pauseMenu.isPaused) return true;
            if (inventoryUI != null && inventoryUI.isUIVisible) return true;
            if (upgradeUI != null && upgradeUI.isUIVisible) return true;
            
            // New systems UI check
            if (RentIsDue.Gameplay.RepairManager.Instance != null && RentIsDue.Gameplay.RepairManager.Instance.IsUIOpen) return true;
            if (RentIsDue.Gameplay.DailyOrderManager.Instance != null && RentIsDue.Gameplay.DailyOrderManager.Instance.IsUIOpen) return true;
            if (RentIsDue.Gameplay.CollectorManager.Instance != null && RentIsDue.Gameplay.CollectorManager.Instance.IsUIOpen) return true;
            if (RentIsDue.Shop.ToolShopManager.Instance != null && RentIsDue.Shop.ToolShopManager.Instance.IsUIOpen) return true;
            
            return false;
        }

        private void SetCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void Update()
        {
            if (playerBody == null)
            {
                FindPlayer();
                if (playerBody == null) return;
            }

            // Kiểm tra xem có đang mở Menu nào không
            if (IsAnyUIOpen())
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    SetCursorState(false);
                }
                return; // Dừng xoay camera khi đang mở menu
            }
            else
            {
                // Khi đang chơi bình thường, khóa chuột lại
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        SetCursorState(true);
                    }
                }
            }

            if (Mouse.current == null) return;

            // Đọc di chuyển của chuột với tốc độ dịu hơn
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float mouseX = mouseDelta.x * mouseSensitivity * 0.04f;
            float mouseY = mouseDelta.y * mouseSensitivity * 0.04f;

            // Xoay đầu lên/xuống (Pitch)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);

            // Xoay thân người chơi trái/phải (Yaw)
            playerBody.Rotate(Vector3.up * mouseX);
        }

        private void LateUpdate()
        {
            if (playerBody == null) return;

            // Tính toán vị trí chân tiếp đất chính xác của nhân vật
            float groundY = playerBody.position.y;
            CharacterController cc = playerBody.GetComponent<CharacterController>();
            if (cc != null)
            {
                groundY = playerBody.position.y + cc.center.y - (cc.height / 2f);
            }

            // Đặt tầm mắt luôn chuẩn xác 1.85m phía trên mặt sàn
            transform.position = new Vector3(playerBody.position.x, groundY + 1.85f, playerBody.position.z);

            // Đồng bộ hướng nhìn của Camera: Xoay dọc theo đầu (xRotation) và xoay ngang theo thân người chơi (playerBody.eulerAngles.y)
            transform.rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
        }

        private void OnGUI()
        {
            // Hiển thị thông số độ cao thực tế ở góc dưới để kiểm chứng
            if (Time.timeScale > 0f)
            {
                GUIStyle style = new GUIStyle(GUI.skin.label);
                style.fontSize = 11;
                style.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
                
                float groundY = playerBody != null ? playerBody.position.y : 0f;
                CharacterController cc = playerBody != null ? playerBody.GetComponent<CharacterController>() : null;
                if (cc != null)
                {
                    groundY = playerBody.position.y + cc.center.y - (cc.height / 2f);
                }

                float currentEyeHeight = transform.position.y - groundY;
                GUI.Label(new Rect(10, Screen.height - 22, 450, 20), $"[Debug] Camera Eye Height: {currentEyeHeight:F2}m (Ground: {groundY:F2}m | FOV: 80°)", style);
            }
        }
    }
}

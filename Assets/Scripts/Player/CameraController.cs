using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Core;

namespace RentIsDue.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Positioning")]
        public Transform playerBody;
        public Vector3 eyeOffset = new Vector3(0, 0.7f, 0); // Vị trí tầm mắt nhân vật

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

            // Đặt vị trí camera luôn bám vào tầm mắt của Player
            transform.position = playerBody.position + eyeOffset;

            // Đồng bộ hướng nhìn của Camera: Xoay dọc theo đầu (xRotation) và xoay ngang theo thân người chơi (playerBody.eulerAngles.y)
            transform.rotation = Quaternion.Euler(xRotation, playerBody.eulerAngles.y, 0f);
        }
    }
}

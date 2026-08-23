using UnityEngine;
using UnityEngine.InputSystem;

namespace RentIsDue.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target & Positioning")]
        public Transform playerBody;
        public Vector3 eyeOffset = new Vector3(0, 0.6f, 0); // Vị trí tầm mắt nhân vật

        [Header("Mouse Look Settings")]
        public float mouseSensitivity = 15f;
        public float minPitch = -85f;
        public float maxPitch = 85f;

        private float xRotation = 0f;

        private PauseMenu pauseMenu;
        private Inventory.InventoryUI inventoryUI;
        private Shop.UpgradeUI upgradeUI;

        private void Start()
        {
            if (playerBody == null)
            {
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) playerBody = player.transform;
            }

            pauseMenu = FindAnyObjectByType<PauseMenu>();
            inventoryUI = FindAnyObjectByType<Inventory.InventoryUI>();
            upgradeUI = FindAnyObjectByType<Shop.UpgradeUI>();

            SetCursorState(true);
        }

        private bool IsAnyUIOpen()
        {
            if (Time.timeScale == 0f) return true;
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
            // Kiểm tra xem có đang mở Menu nào không (Pause, Inventory Tab, Upgrade Laptop)
            if (IsAnyUIOpen())
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    SetCursorState(false);
                }
                return; // Dừng xoay camera khi đang mở menu để dùng chuột bấm nút
            }
            else
            {
                // Khi không mở menu nào, tự động khóa chuột lại để xoay góc nhìn
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    SetCursorState(true);
                }
            }

            if (playerBody == null || Mouse.current == null) return;

            // Đọc di chuyển của chuột
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * (mouseSensitivity * 0.05f);

            float mouseX = mouseDelta.x;
            float mouseY = mouseDelta.y;

            // Xoay đầu lên/xuống (Pitch)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Xoay thân người sang trái/phải (Yaw)
            playerBody.Rotate(Vector3.up * mouseX);
        }

        private void LateUpdate()
        {
            if (playerBody == null) return;

            // Camera luôn bám theo vị trí tầm mắt của người chơi
            transform.position = playerBody.position + eyeOffset;
        }
    }
}

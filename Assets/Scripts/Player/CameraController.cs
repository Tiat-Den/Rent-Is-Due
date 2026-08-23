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

        private void Start()
        {
            if (playerBody == null)
            {
                PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
                if (player != null) playerBody = player.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            // Nếu game bị Pause (Time.timeScale == 0) thì mở khóa chuột để bấm UI
            if (Time.timeScale == 0f)
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                return;
            }
            else
            {
                // Khi đang chơi bình thường, khóa con trỏ chuột lại
                if (Cursor.lockState != CursorLockMode.Locked)
                {
                    // Nếu click chuột vào màn hình thì khóa chuột lại
                    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
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

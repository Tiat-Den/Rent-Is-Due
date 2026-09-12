using UnityEngine;
using UnityEngine.InputSystem;

namespace RentIsDue.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 6f;
        public float jumpHeight = 1.3f;
        public float gravityValue = -18f;
        
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool isGrounded;
        private StaminaSystem staminaSystem;

        private void Awake()
        {
            // Gỡ bỏ hoàn toàn CapsuleCollider thừa trên Player để tránh xung đột vật lý với CharacterController
            Collider[] colliders = GetComponents<Collider>();
            foreach (var col in colliders)
            {
                if (!(col is CharacterController))
                {
                    Destroy(col);
                }
            }

            // Gỡ bỏ MeshRenderer hình viên thuốc của Player để góc nhìn FPS trong suốt hoàn toàn
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null) Destroy(mr);
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null) Destroy(mf);
        }

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.skinWidth = 0.05f;
                controller.minMoveDistance = 0f;
            }

            staminaSystem = GetComponent<StaminaSystem>();
            if (staminaSystem == null)
            {
                staminaSystem = gameObject.AddComponent<StaminaSystem>();
            }
        }

        private void Update()
        {
            if (controller == null) return;

            // Cơ chế cứu nguy rơi vực / văng khỏi bản đồ (Map Safeguard):
            // Nếu bị va chạm đẩy văng lên trời (> 12m), rơi xuống vực (< -5m) hoặc bay ra ngoài biên tường
            if (transform.position.y < -5f || transform.position.y > 12f || Mathf.Abs(transform.position.x) > 28f || transform.position.z < -20f || transform.position.z > 65f)
            {
                controller.enabled = false;
                transform.position = new Vector3(0, 1.2f, 0);
                playerVelocity = Vector3.zero;
                controller.enabled = true;
                return;
            }

            isGrounded = controller.isGrounded;
            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = -2f; // Giữ lực đè nhẹ để bám sàn chắc chắn
            }

            // Đọc phím di chuyển W, A, S, D
            float inputX = 0f;
            float inputZ = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputZ += 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputZ -= 1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputX -= 1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputX += 1f;
            }

            // Tính hướng di chuyển theo đúng góc nhìn của nhân vật (FPS)
            Vector3 move = transform.right * inputX + transform.forward * inputZ;

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            // Tốc độ chạy nhanh (Sprint) khi giữ Shift
            float currentSpeed = moveSpeed;
            if (staminaSystem != null && staminaSystem.isSprinting && move.sqrMagnitude > 0.01f)
            {
                currentSpeed = moveSpeed * 1.55f; // Chạy nhanh hơn 55%
            }

            controller.Move(move * currentSpeed * Time.deltaTime);

            // Nhảy khi bấm Space (tiêu tốn 1 ít thể lực)
            if (isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (staminaSystem == null || staminaSystem.CanJump())
                {
                    playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
                    if (staminaSystem != null) staminaSystem.ConsumeJumpStamina();
                }
            }

            // Trọng lực
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }
}

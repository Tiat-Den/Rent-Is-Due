using UnityEngine;
using UnityEngine.InputSystem;

namespace RentIsDue.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpHeight = 1.2f;
        public float gravityValue = -15f;
        
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;

        // Sử dụng InputAction trực tiếp trong code để không cần tạo file cấu hình Input ngoài
        private InputAction moveAction;

        private void Start()
        {
            controller = GetComponent<CharacterController>();
            
            moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
            moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            
            moveAction.Enable();
        }

        private void OnDestroy()
        {
            moveAction?.Disable();
        }

        private void Update()
        {
            groundedPlayer = controller.isGrounded;
            if (groundedPlayer && playerVelocity.y < 0)
            {
                // Giữ một lực đè nhỏ để bám đất ổn định trên dốc/bề mặt
                playerVelocity.y = -2f;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();
            // Di chuyển theo hướng nhân vật đang nhìn (First-Person)
            Vector3 move = transform.right * input.x + transform.forward * input.y;

            // Normalize để đi chéo không bị nhanh hơn
            if (move.magnitude > 1f) move.Normalize();

            controller.Move(move * Time.deltaTime * moveSpeed);

            // Xử lý nhảy (Space)
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && groundedPlayer)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            }

            // Xử lý trọng lực
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace RentIsDue.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float rotationSpeed = 10f;
        
        private CharacterController controller;
        private Vector3 playerVelocity;
        private bool groundedPlayer;
        private float gravityValue = -9.81f;

        // Sử dụng InputAction trực tiếp trong code để bạn không cần tạo file cấu hình Input ngoài
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
                playerVelocity.y = 0f;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();
            // Di chuyển trên mặt phẳng XZ (3D top-down)
            Vector3 move = new Vector3(input.x, 0, input.y);

            // Normalize để đi chéo không bị nhanh hơn
            if (move.magnitude > 1f) move.Normalize();

            controller.Move(move * Time.deltaTime * moveSpeed);

            if (move != Vector3.zero)
            {
                // Xoay nhân vật mượt mà theo hướng di chuyển
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }

            // Xử lý trọng lực cơ bản
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }
}

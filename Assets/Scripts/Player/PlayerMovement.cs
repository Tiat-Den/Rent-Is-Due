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

        private void Start()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            isGrounded = controller.isGrounded;
            if (isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = -2f; // Giữ lực đè nhẹ để bám đất
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

            controller.Move(move * moveSpeed * Time.deltaTime);

            // Nhảy khi bấm Space
            if (isGrounded && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravityValue);
            }

            // Trọng lực
            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
    }
}

using UnityEngine;

namespace RentIsDue.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        public Transform target;
        
        [Header("Settings")]
        // Góc nghiêng mặc định cho game Isometric / Top-down
        public Vector3 offset = new Vector3(0, 8f, -6f); 
        public float smoothSpeed = 5f;

        private void LateUpdate()
        {
            if (target == null) return;
            
            // Tính toán vị trí mong muốn
            Vector3 desiredPosition = target.position + offset;
            
            // Di chuyển mượt (Lerp)
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Luôn nhìn về phía nhân vật
            transform.LookAt(target);
        }
    }
}

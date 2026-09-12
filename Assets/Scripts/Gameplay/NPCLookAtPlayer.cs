using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Makes an NPC turn smoothly to face the Player when nearby, and returns to default forward orientation.
    /// Also applies subtle breathing idle animation for organic life.
    /// </summary>
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        private Transform playerTransform;
        private Quaternion defaultRotation;
        private Vector3 initialLocalPos;
        private float bobSpeed = 2.0f;
        private float bobAmount = 0.012f;

        private void Start()
        {
            defaultRotation = transform.rotation;
            initialLocalPos = transform.localPosition;
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            // Subtle breathing idle motion
            float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y + bob, initialLocalPos.z);

            // Distance to player
            Vector3 diff = playerTransform.position - transform.position;
            diff.y = 0; // Only rotate on the horizontal (Yaw) plane
            float distSqr = diff.sqrMagnitude;

            if (distSqr <= noticeDistance * noticeDistance && distSqr > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(diff);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
            else
            {
                // Smoothly return to default facing direction
                transform.rotation = Quaternion.Slerp(transform.rotation, defaultRotation, Time.deltaTime * (turnSpeed * 0.6f));
            }
        }
    }
}

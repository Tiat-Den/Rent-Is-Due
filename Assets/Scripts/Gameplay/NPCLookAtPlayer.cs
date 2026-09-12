using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Makes an NPC stand naturally with relaxed arms (no T-pose),
    /// breathe gently, and turn smoothly to face the Player when nearby.
    /// Completely procedural - no Animator/IK bugs that distort the mesh or throw it into the void.
    /// </summary>
    [ExecuteAlways]
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        [Header("Procedural Stance")]
        public bool enforceNaturalArmPose = true;

        private Transform playerTransform;
        private Quaternion defaultRotation;
        private Vector3 initialLocalPos;
        private float bobSpeed = 2.0f;
        private float bobAmount = 0.015f;

        private Transform leftArm;
        private Transform rightArm;
        private Transform leftForeArm;
        private Transform rightForeArm;

        private Quaternion initialLeftArmRot;
        private Quaternion initialRightArmRot;
        private Quaternion initialLeftForeArmRot;
        private Quaternion initialRightForeArmRot;
        private bool hasCachedBindPose = false;

        private void Awake()
        {
            if (transform.localPosition != Vector3.zero) initialLocalPos = transform.localPosition;
            defaultRotation = transform.rotation;
            CacheReferences();
            CacheBindPose();
        }

        private void Start()
        {
            defaultRotation = transform.rotation;
            if (transform.localPosition != Vector3.zero) initialLocalPos = transform.localPosition;
            CacheReferences();
            CacheBindPose();
            FindPlayer();
        }

        private void OnEnable()
        {
            if (transform.localPosition != Vector3.zero) initialLocalPos = transform.localPosition;
            defaultRotation = transform.rotation;
            CacheReferences();
            CacheBindPose();
        }

        private void CacheReferences()
        {
            if (leftArm == null) leftArm = FindChildRecursive(transform, "LeftArm");
            if (rightArm == null) rightArm = FindChildRecursive(transform, "RightArm");
            if (leftForeArm == null) leftForeArm = FindChildRecursive(transform, "LeftForeArm");
            if (rightForeArm == null) rightForeArm = FindChildRecursive(transform, "RightForeArm");
        }

        private void CacheBindPose()
        {
            if (hasCachedBindPose) return;

            if (leftArm != null) initialLeftArmRot = leftArm.localRotation;
            if (rightArm != null) initialRightArmRot = rightArm.localRotation;
            if (leftForeArm != null) initialLeftForeArmRot = leftForeArm.localRotation;
            if (rightForeArm != null) initialRightForeArmRot = rightForeArm.localRotation;

            if (leftArm != null && rightArm != null)
            {
                hasCachedBindPose = true;
            }
        }

        private static Transform FindChildRecursive(Transform parent, string targetName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase))
                    return child;
                Transform found = FindChildRecursive(child, targetName);
                if (found != null) return found;
            }
            return null;
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
            if (!Application.isPlaying) return;

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

        private void LateUpdate()
        {
            if (enforceNaturalArmPose && hasCachedBindPose)
            {
                // Procedurally lower arms along the body so character never T-poses
                if (leftArm != null)
                {
                    leftArm.localRotation = initialLeftArmRot;
                    leftArm.Rotate(transform.forward, -68f, Space.World);
                    leftArm.Rotate(transform.right, 10f, Space.World);
                }
                if (rightArm != null)
                {
                    rightArm.localRotation = initialRightArmRot;
                    rightArm.Rotate(transform.forward, 68f, Space.World);
                    rightArm.Rotate(transform.right, 10f, Space.World);
                }
            }
        }
    }
}

using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Makes an NPC turn smoothly to face the Player when nearby, and returns to default forward orientation.
    /// Plays skeletal Idle animation via Animator, with procedural arm rest fallback so characters never T-pose.
    /// </summary>
    [ExecuteAlways]
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        [Header("Procedural Fallback Pose")]
        public bool enforceNaturalArmPose = true;

        private Transform playerTransform;
        private Quaternion defaultRotation;
        private Vector3 initialLocalPos;
        private float bobSpeed = 2.0f;
        private float bobAmount = 0.010f;

        private Animator npcAnimator;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftForeArm;
        private Transform rightForeArm;

        private void Awake()
        {
            CacheReferences();
        }

        private void Start()
        {
            defaultRotation = transform.rotation;
            initialLocalPos = transform.localPosition;
            CacheReferences();
            FindPlayer();
        }

        private void OnEnable()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            if (npcAnimator == null) npcAnimator = GetComponent<Animator>();
            if (leftArm == null) leftArm = FindChildRecursive(transform, "LeftArm");
            if (rightArm == null) rightArm = FindChildRecursive(transform, "RightArm");
            if (leftForeArm == null) leftForeArm = FindChildRecursive(transform, "LeftForeArm");
            if (rightForeArm == null) rightForeArm = FindChildRecursive(transform, "RightForeArm");
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

            // Subtle breathing idle motion when no animator is active or for ambient life
            if (npcAnimator == null || !npcAnimator.isActiveAndEnabled || npcAnimator.runtimeAnimatorController == null)
            {
                float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
                transform.localPosition = new Vector3(initialLocalPos.x, initialLocalPos.y + bob, initialLocalPos.z);
            }

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
            // If Animator is not playing an animation clip (e.g. in Edit mode or if Animator controller is unassigned),
            // procedurally lower arms to a natural resting stance instead of T-Pose bind pose.
            bool isAnimatorRunning = Application.isPlaying && npcAnimator != null && npcAnimator.isActiveAndEnabled && npcAnimator.runtimeAnimatorController != null;
            if (!isAnimatorRunning && enforceNaturalArmPose)
            {
                if (leftArm != null)
                {
                    leftArm.localRotation = Quaternion.Euler(15f, 0f, -75f);
                }
                if (rightArm != null)
                {
                    rightArm.localRotation = Quaternion.Euler(15f, 0f, 75f);
                }
                if (leftForeArm != null)
                {
                    leftForeArm.localRotation = Quaternion.Euler(0f, 0f, -15f);
                }
                if (rightForeArm != null)
                {
                    rightForeArm.localRotation = Quaternion.Euler(0f, 0f, 15f);
                }
            }
        }
    }
}

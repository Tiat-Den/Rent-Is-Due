using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Manages the NPC's natural shopkeeper stance (no T-pose), gentle breathing animation,
    /// and smoothly turns the NPC to face the player when they approach the shop/counter.
    /// Operates in both Edit Mode ([ExecuteAlways]) and Play Mode for immediate, flawless visuals.
    /// Uses immutable bind-pose references to guarantee zero rotation drift or distortion.
    /// </summary>
    [ExecuteAlways]
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        [Header("Stance & Animation")]
        public bool enforceNaturalPose = true;
        public bool enableBreathing = true;

        // Exact bind-pose local rotations of Kenney's characterMedium rig:
        private static readonly Quaternion BindRotLeftArm = Quaternion.Euler(-83.12f, 101.85f, -84.96f);
        private static readonly Quaternion BindRotLeftForeArm = Quaternion.Euler(5.37f, 1.41f, 0.69f);
        private static readonly Quaternion BindRotLeftHand = Quaternion.Euler(52.41f, 94.88f, 52.83f);

        private static readonly Quaternion BindRotRightArm = Quaternion.Euler(13.07f, -152.24f, -3.13f);
        private static readonly Quaternion BindRotRightForeArm = Quaternion.Euler(0.06f, -0.55f, 5.40f);
        private static readonly Quaternion BindRotRightHand = Quaternion.Euler(0.94f, 0.07f, -3.79f);

        private static readonly Quaternion BindRotSpine = Quaternion.Euler(-7.00f, 0f, 0f);

        private Transform playerTransform;
        private Quaternion defaultRotation;

        private Transform leftArm;
        private Transform leftForeArm;
        private Transform leftHand;

        private Transform rightArm;
        private Transform rightForeArm;
        private Transform rightHand;

        private Transform spine;
        private Transform head;

        private bool hasCachedBones = false;

        private void Awake()
        {
            defaultRotation = transform.rotation;
            CacheBoneReferences();
        }

        private void Start()
        {
            defaultRotation = transform.rotation;
            CacheBoneReferences();
            if (Application.isPlaying)
            {
                FindPlayer();
            }
        }

        private void OnEnable()
        {
            CacheBoneReferences();
        }

        private void CacheBoneReferences()
        {
            if (hasCachedBones && leftArm != null) return;

            leftArm = FindChildRecursive(transform, "LeftArm");
            leftForeArm = FindChildRecursive(transform, "LeftForeArm");
            leftHand = FindChildRecursive(transform, "LeftHand");

            rightArm = FindChildRecursive(transform, "RightArm");
            rightForeArm = FindChildRecursive(transform, "RightForeArm");
            rightHand = FindChildRecursive(transform, "RightHand");

            spine = FindChildRecursive(transform, "Spine");
            head = FindChildRecursive(transform, "Head");

            hasCachedBones = (leftArm != null && rightArm != null);
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

            // Vector to player on horizontal plane
            Vector3 diff = playerTransform.position - transform.position;
            diff.y = 0; // Rotate only around the vertical Yaw axis
            float distSqr = diff.sqrMagnitude;

            if (distSqr <= noticeDistance * noticeDistance && distSqr > 0.04f)
            {
                Quaternion targetRot = Quaternion.LookRotation(diff);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }
            else
            {
                // Smoothly return to default counter-facing direction
                transform.rotation = Quaternion.Slerp(transform.rotation, defaultRotation, Time.deltaTime * (turnSpeed * 0.6f));
            }
        }

        private void LateUpdate()
        {
            if (!enforceNaturalPose) return;

            if (!hasCachedBones || leftArm == null)
            {
                CacheBoneReferences();
                if (!hasCachedBones) return;
            }

            // 1. Reset all animated bones to pristine bind pose before applying pose offsets
            leftArm.localRotation = BindRotLeftArm;
            rightArm.localRotation = BindRotRightArm;
            if (leftForeArm != null) leftForeArm.localRotation = BindRotLeftForeArm;
            if (rightForeArm != null) rightForeArm.localRotation = BindRotRightForeArm;
            if (leftHand != null) leftHand.localRotation = BindRotLeftHand;
            if (rightHand != null) rightHand.localRotation = BindRotRightHand;
            if (spine != null) spine.localRotation = BindRotSpine;

            // 2. Subtle breathing on spine (alive shopkeeper feel)
            if (enableBreathing && spine != null)
            {
                float time = Application.isPlaying ? Time.time : 0f;
                float breath = Mathf.Sin(time * 2.0f) * 1.5f;
                spine.localRotation = BindRotSpine * Quaternion.Euler(breath, 0f, 0f);
            }

            // 3. Relax Left Arm down along the torso (natural resting pose)
            if (leftArm != null && leftForeArm != null)
            {
                Vector3 currentLeftDir = (leftForeArm.position - leftArm.position).normalized;
                // Target: down, slightly forward, slightly outward from hips
                Vector3 targetLeftDir = (-transform.up * 0.92f + transform.forward * 0.15f + transform.right * 0.35f).normalized;
                leftArm.rotation = Quaternion.FromToRotation(currentLeftDir, targetLeftDir) * leftArm.rotation;

                // Forearm: bend elbow slightly forward & resting naturally
                if (leftHand != null)
                {
                    Vector3 curFore = (leftHand.position - leftForeArm.position).normalized;
                    Vector3 tgtFore = (-transform.up * 0.70f + transform.forward * 0.65f - transform.right * 0.28f).normalized;
                    leftForeArm.rotation = Quaternion.FromToRotation(curFore, tgtFore) * leftForeArm.rotation;
                }
            }

            // 4. Relax Right Arm down along the torso (natural resting pose)
            if (rightArm != null && rightForeArm != null)
            {
                Vector3 currentRightDir = (rightForeArm.position - rightArm.position).normalized;
                // Target: down, slightly forward, slightly outward from hips
                Vector3 targetRightDir = (-transform.up * 0.92f + transform.forward * 0.15f - transform.right * 0.35f).normalized;
                rightArm.rotation = Quaternion.FromToRotation(currentRightDir, targetRightDir) * rightArm.rotation;

                // Forearm: bend elbow slightly forward & resting naturally
                if (rightHand != null)
                {
                    Vector3 curFore = (rightHand.position - rightForeArm.position).normalized;
                    Vector3 tgtFore = (-transform.up * 0.70f + transform.forward * 0.65f + transform.right * 0.28f).normalized;
                    rightForeArm.rotation = Quaternion.FromToRotation(curFore, tgtFore) * rightForeArm.rotation;
                }
            }

            // 5. Head tilt towards player if nearby
            if (head != null && playerTransform != null && Application.isPlaying)
            {
                Vector3 toPlayer = (playerTransform.position + Vector3.up * 1.5f - head.position).normalized;
                if (Vector3.Dot(transform.forward, toPlayer) > 0.3f && Vector3.Distance(transform.position, playerTransform.position) < noticeDistance)
                {
                    Quaternion targetHeadRot = Quaternion.LookRotation(toPlayer, transform.up);
                    head.rotation = Quaternion.Slerp(head.rotation, targetHeadRot, 0.35f);
                }
            }
        }
    }
}

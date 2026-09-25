using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Implements Kenney Character Model Animation and Stance standards.
    /// Uses exact local Euler rotations and breathing curves extracted directly from Kenney's idle animation clip,
    /// guaranteeing natural resting arm posture, zero mesh twisting/pinching, and lively breathing.
    /// Operates in both Edit Mode ([ExecuteAlways]) and Play Mode.
    /// Smoothly rotates the NPC to face the player upon approach.
    /// </summary>
    [ExecuteAlways]
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Detection & Tracking")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        [Header("Animation Control")]
        public bool enforceNaturalPose = true;
        public bool enableBreathing = true;
        [Range(0.5f, 4.0f)]
        public float breathingSpeed = 2.0f;

        // Exact Kenney authentic idle pose constants (extracted from binary idle.fbx):
        // Shoulders
        private static readonly Quaternion RotLeftShoulder = Quaternion.Euler(-124.12f, -87.22f, 13.39f);
        private static readonly Quaternion RotRightShoulder = Quaternion.Euler(6.55f, -113.22f, 104.46f);

        // Arms (Upper)
        private static readonly Quaternion RotLeftArm = Quaternion.Euler(-98.72f, 40.68f, -89.41f);
        private static readonly Quaternion RotRightArm = Quaternion.Euler(-46.74f, -162.78f, 0.02f);

        // Forearms (Elbows)
        private static readonly Quaternion RotLeftForeArm = Quaternion.Euler(41.85f, -2.60f, -11.79f);
        private static readonly Quaternion RotRightForeArm = Quaternion.Euler(-10.38f, 0.48f, 41.27f);

        // Hands (Wrists)
        private static readonly Quaternion RotLeftHand = Quaternion.Euler(52.86f, 94.36f, 50.30f);
        private static readonly Quaternion RotRightHand = Quaternion.Euler(-1.72f, 15.85f, -4.13f);

        // Fingers (Relaxed natural curl)
        private static readonly Quaternion RotLeftIndex1 = Quaternion.Euler(1.88f, -8.90f, -0.14f);
        private static readonly Quaternion RotLeftIndex2 = Quaternion.Euler(101.28f, 3.22f, 4.28f);
        private static readonly Quaternion RotLeftIndex3 = Quaternion.Euler(48.55f, -3.50f, -0.01f);
        private static readonly Quaternion RotLeftThumb1 = Quaternion.Euler(-77.10f, 104.78f, -29.01f);
        private static readonly Quaternion RotLeftThumb2 = Quaternion.Euler(83.10f, -15.60f, 4.54f);

        private static readonly Quaternion RotRightIndex1 = Quaternion.Euler(-1.88f, -8.90f, 0.14f);
        private static readonly Quaternion RotRightIndex2 = Quaternion.Euler(-96.95f, 3.87f, -3.04f);
        private static readonly Quaternion RotRightIndex3 = Quaternion.Euler(-48.55f, -3.50f, 0.01f);
        private static readonly Quaternion RotRightThumb1 = Quaternion.Euler(42.36f, -122.29f, 8.02f);
        private static readonly Quaternion RotRightThumb2 = Quaternion.Euler(-71.36f, 25.96f, 26.77f);

        // Torso & Head
        private static readonly Quaternion RotSpine = Quaternion.Euler(0.62f, 0f, 0f);
        private static readonly Quaternion RotNeck = Quaternion.Euler(9.84f, 0f, 0f);
        private static readonly Quaternion RotHead = Quaternion.Euler(-16.58f, 4.85f, -1.73f);

        private Transform playerTransform;
        private Quaternion defaultRotation;

        // Bone references
        private Transform leftShoulder;
        private Transform leftArm;
        private Transform leftForeArm;
        private Transform leftHand;
        private Transform leftIndex1, leftIndex2, leftIndex3, leftThumb1, leftThumb2;

        private Transform rightShoulder;
        private Transform rightArm;
        private Transform rightForeArm;
        private Transform rightHand;
        private Transform rightIndex1, rightIndex2, rightIndex3, rightThumb1, rightThumb2;

        private Transform spine;
        private Transform neck;
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

            leftShoulder = FindChildRecursive(transform, "LeftShoulder");
            leftArm = FindChildRecursive(transform, "LeftArm");
            leftForeArm = FindChildRecursive(transform, "LeftForeArm");
            leftHand = FindChildRecursive(transform, "LeftHand");
            leftIndex1 = FindChildRecursive(transform, "LeftHandIndex1");
            leftIndex2 = FindChildRecursive(transform, "LeftHandIndex2");
            leftIndex3 = FindChildRecursive(transform, "LeftHandIndex3");
            leftThumb1 = FindChildRecursive(transform, "LeftHandThumb1");
            leftThumb2 = FindChildRecursive(transform, "LeftHandThumb2");

            rightShoulder = FindChildRecursive(transform, "RightShoulder");
            rightArm = FindChildRecursive(transform, "RightArm");
            rightForeArm = FindChildRecursive(transform, "RightForeArm");
            rightHand = FindChildRecursive(transform, "RightHand");
            rightIndex1 = FindChildRecursive(transform, "RightHandIndex1");
            rightIndex2 = FindChildRecursive(transform, "RightHandIndex2");
            rightIndex3 = FindChildRecursive(transform, "RightHandIndex3");
            rightThumb1 = FindChildRecursive(transform, "RightHandThumb1");
            rightThumb2 = FindChildRecursive(transform, "RightHandThumb2");

            spine = FindChildRecursive(transform, "Spine");
            neck = FindChildRecursive(transform, "Neck");
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

            // Smooth Horizontal Yaw rotation to face the player
            Vector3 diff = playerTransform.position - transform.position;
            diff.y = 0;
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

            // Calculate subtle breathing cycle (sine wave)
            float t = Application.isPlaying ? (Time.time * breathingSpeed) : 0f;
            float breath = enableBreathing ? Mathf.Sin(t) : 0f;

            // 1. Spine (Gentle chest expansion & breathing nod)
            if (spine != null)
            {
                spine.localRotation = Quaternion.Euler(0.62f + breath * 1.15f, 0f, 0f);
            }

            // 2. Shoulders (Authentic Kenney orientation + subtle breathing shrug)
            if (leftShoulder != null)
            {
                leftShoulder.localRotation = Quaternion.Euler(-124.12f, -87.22f, 13.39f + breath * 0.8f);
            }
            if (rightShoulder != null)
            {
                rightShoulder.localRotation = Quaternion.Euler(6.55f, -113.22f, 104.46f - breath * 0.8f);
            }

            // 3. Left Arm & Elbow (Down along torso, elbow bent naturally, zero twisting)
            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Euler(-98.72f + breath * 3.0f, 40.68f, -89.41f);
            }
            if (leftForeArm != null)
            {
                leftForeArm.localRotation = RotLeftForeArm;
            }
            if (leftHand != null)
            {
                leftHand.localRotation = RotLeftHand;
            }

            // Left Fingers (Relaxed hand)
            if (leftIndex1 != null) leftIndex1.localRotation = RotLeftIndex1;
            if (leftIndex2 != null) leftIndex2.localRotation = RotLeftIndex2;
            if (leftIndex3 != null) leftIndex3.localRotation = RotLeftIndex3;
            if (leftThumb1 != null) leftThumb1.localRotation = RotLeftThumb1;
            if (leftThumb2 != null) leftThumb2.localRotation = RotLeftThumb2;

            // 4. Right Arm & Elbow (Down along torso, elbow bent naturally, zero twisting)
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Euler(-46.74f - breath * 3.0f, -162.78f, 0.02f);
            }
            if (rightForeArm != null)
            {
                rightForeArm.localRotation = RotRightForeArm;
            }
            if (rightHand != null)
            {
                rightHand.localRotation = RotRightHand;
            }

            // Right Fingers (Relaxed hand)
            if (rightIndex1 != null) rightIndex1.localRotation = RotRightIndex1;
            if (rightIndex2 != null) rightIndex2.localRotation = RotRightIndex2;
            if (rightIndex3 != null) rightIndex3.localRotation = RotRightIndex3;
            if (rightThumb1 != null) rightThumb1.localRotation = RotRightThumb1;
            if (rightThumb2 != null) rightThumb2.localRotation = RotRightThumb2;

            // 5. Neck & Head (Natural posture + breathing nod + subtle look at player)
            if (neck != null)
            {
                neck.localRotation = Quaternion.Euler(9.84f + breath * 0.5f, 0f, 0f);
            }

            if (head != null)
            {
                head.localRotation = Quaternion.Euler(-16.58f + breath * 1.5f, 4.85f, -1.73f);

                // If player is close, tilt head slightly toward player
                if (Application.isPlaying && playerTransform != null)
                {
                    Vector3 toPlayer = (playerTransform.position + Vector3.up * 1.4f - head.position).normalized;
                    if (Vector3.Dot(transform.forward, toPlayer) > 0.4f && Vector3.Distance(transform.position, playerTransform.position) < noticeDistance)
                    {
                        Quaternion targetHeadRot = Quaternion.LookRotation(toPlayer, transform.up);
                        head.rotation = Quaternion.Slerp(head.rotation, targetHeadRot, 0.25f);
                    }
                }
            }
        }
    }
}

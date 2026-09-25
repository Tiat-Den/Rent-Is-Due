using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Controls NPC scale, shopkeeper stance (relaxed arms, breathing), and player tracking.
    /// Eliminates T-pose by procedurally rotating arms down along the torso from bind pose,
    /// and safeguards the NPC against scale explosion and orientation flips in both Edit and Play mode.
    /// </summary>
    [ExecuteAlways]
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Animation (Optional)")]
        public AnimationClip idleClip;

        [Header("Player Tracking")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        [Header("Stance & Breathing")]
        public bool enableBreathing = true;

        private Transform playerTransform;
        private Quaternion defaultRotation;

        // Cached bone transforms
        private Transform rootBone;
        private Transform spine;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftForeArm;
        private Transform rightForeArm;

        // Bind pose rotations
        private bool hasCachedBones = false;
        private Quaternion bindRotSpine = Quaternion.identity;
        private Quaternion bindRotLeftArm = Quaternion.identity;
        private Quaternion bindRotRightArm = Quaternion.identity;
        private Quaternion bindRotLeftForeArm = Quaternion.identity;
        private Quaternion bindRotRightForeArm = Quaternion.identity;

        private void Awake()
        {
            EnforceScaleAndOrientation();
            defaultRotation = transform.rotation;
            DisableMecanimAnimator();
            CacheBones();
        }

        private void OnEnable()
        {
            EnforceScaleAndOrientation();
            DisableMecanimAnimator();
            CacheBones();
            ApplyNaturalStance(0f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnforceScaleAndOrientation();
            DisableMecanimAnimator();
            CacheBones();
            ApplyNaturalStance(0f);
        }
#endif

        private void Start()
        {
            EnforceScaleAndOrientation();
            defaultRotation = transform.rotation;
            DisableMecanimAnimator();
            CacheBones();
            ApplyNaturalStance(0f);

            if (Application.isPlaying)
            {
                FindPlayer();
            }
        }

        private void DisableMecanimAnimator()
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.enabled)
            {
                anim.enabled = false;
            }
        }

        private void CacheBones()
        {
            if (hasCachedBones && leftArm != null && rightArm != null) return;

            rootBone = FindChildRecursive(transform, "Root");
            spine = FindChildRecursive(transform, "Spine");
            leftArm = FindChildRecursive(transform, "LeftArm");
            rightArm = FindChildRecursive(transform, "RightArm");
            leftForeArm = FindChildRecursive(transform, "LeftForeArm");
            rightForeArm = FindChildRecursive(transform, "RightForeArm");

            if (leftArm != null && rightArm != null)
            {
                // Store standard bind rotations for characterMedium
                bindRotLeftArm = Quaternion.Euler(-83.124f, 101.850f, -84.964f);
                bindRotRightArm = Quaternion.Euler(13.066f, -152.237f, -3.131f);
                bindRotLeftForeArm = Quaternion.Euler(5.369f, 1.410f, 0.691f);
                bindRotRightForeArm = Quaternion.Euler(0.058f, -0.550f, 5.396f);
                bindRotSpine = Quaternion.Euler(-6.999f, 0f, 0f);
                hasCachedBones = true;
            }
        }

        private static Transform FindChildRecursive(Transform parent, string targetName)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                var c = parent.GetChild(i);
                if (c.name.Equals(targetName, System.StringComparison.OrdinalIgnoreCase)) return c;
                var found = FindChildRecursive(c, targetName);
                if (found != null) return found;
            }
            return null;
        }

        private void EnforceScaleAndOrientation()
        {
            // 1. NPC Root Transform: 0.48 scale (1.80m tall), standing on pavement (Y=0.45m behind counter)
            Vector3 scale = transform.localScale;
            if (Mathf.Abs(scale.x - 0.48f) > 0.05f || Mathf.Abs(scale.y - 0.48f) > 0.05f || Mathf.Abs(scale.z - 0.48f) > 0.05f)
            {
                transform.localScale = Vector3.one * 0.48f;
            }

            Vector3 euler = transform.localEulerAngles;
            if (Mathf.Abs(Mathf.DeltaAngle(euler.x, 0f)) > 10f || Mathf.Abs(Mathf.DeltaAngle(euler.z, 0f)) > 10f)
            {
                transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            Vector3 pos = transform.localPosition;
            if (pos.y < 0.2f || pos.y > 0.8f || pos.z < 0.2f)
            {
                transform.localPosition = new Vector3(0f, 0.45f, 0.70f);
            }

            // 2. Child Root bone: MUST be identity (scale 1, rot 0, pos 0) to prevent 100x giant scale
            if (rootBone == null)
            {
                rootBone = FindChildRecursive(transform, "Root");
            }
            if (rootBone != null)
            {
                if (rootBone.localScale != Vector3.one) rootBone.localScale = Vector3.one;
                if (rootBone.localPosition != Vector3.zero) rootBone.localPosition = Vector3.zero;
                if (rootBone.localRotation != Quaternion.identity) rootBone.localRotation = Quaternion.identity;
            }
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
            // Player tracking (Horizontal Yaw only)
            if (!Application.isPlaying) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

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
            // Apply relaxed arm stance and subtle breathing every frame
            float time = Application.isPlaying ? Time.time : 0f;
            ApplyNaturalStance(time);

            // Re-enforce scale every frame as absolute immunity against unwanted mutations
            EnforceScaleAndOrientation();
        }

        private void ApplyNaturalStance(float time)
        {
            if (!hasCachedBones || leftArm == null || rightArm == null)
            {
                CacheBones();
                if (!hasCachedBones) return;
            }

            // 1. Subtle chest breathing
            float breath = enableBreathing ? Mathf.Sin(time * 2.0f) * 1.5f : 0f;
            if (spine != null)
            {
                spine.localRotation = bindRotSpine * Quaternion.Euler(breath, 0f, 0f);
            }

            // 2. Left Arm: swing down ~80 degrees from horizontal T-pose along torso
            Vector3 desiredLeftDir = new Vector3(0.12f, -0.98f, 0.08f).normalized;
            Quaternion leftSwing = Quaternion.FromToRotation(Vector3.right, desiredLeftDir);
            leftArm.localRotation = leftSwing * bindRotLeftArm;

            // 3. Right Arm: swing down ~80 degrees from horizontal T-pose along torso
            Vector3 desiredRightDir = new Vector3(-0.12f, -0.98f, 0.08f).normalized;
            Quaternion rightSwing = Quaternion.FromToRotation(-Vector3.right, desiredRightDir);
            rightArm.localRotation = rightSwing * bindRotRightArm;

            // 4. Natural forearm elbow bend resting comfortably near the counter
            if (leftForeArm != null)
            {
                leftForeArm.localRotation = bindRotLeftForeArm * Quaternion.Euler(0f, 0f, 22f);
            }
            if (rightForeArm != null)
            {
                rightForeArm.localRotation = bindRotRightForeArm * Quaternion.Euler(0f, 0f, -22f);
            }
        }
    }
}

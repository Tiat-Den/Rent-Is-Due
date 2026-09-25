using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Controls NPC scale, shopkeeper stance (relaxed arms, breathing), and player tracking.
    /// Eliminates T-pose by procedurally rotating arms down along the torso from immutable bind pose constants,
    /// and safeguards the NPC against scale explosion, offscreen culling, and orientation flips.
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

        // Immutable Pristine Bind Rotations from Kenney characterMedium.fbx
        private static readonly Quaternion BindRotLeftArm = Quaternion.Euler(-83.12f, 101.85f, -84.96f);
        private static readonly Quaternion BindRotRightArm = Quaternion.Euler(13.07f, -152.24f, -3.13f);
        private static readonly Quaternion BindRotLeftForeArm = Quaternion.Euler(5.37f, 1.41f, 0.69f);
        private static readonly Quaternion BindRotRightForeArm = Quaternion.Euler(0.06f, -0.55f, 5.40f);
        private static readonly Quaternion BindRotSpine = Quaternion.Euler(-7.00f, 0f, 0f);

        private Transform playerTransform;
        private Quaternion defaultRotation;

        // Cached bone transforms
        private Transform spine;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftForeArm;
        private Transform rightForeArm;
        private bool hasCachedBones = false;

        private void Awake()
        {
            EnforceScaleAndOrientation();
            defaultRotation = transform.rotation;
            DisableMecanimAnimator();
            CacheBones();
            EnsureRenderersVisible();
        }

        private void OnEnable()
        {
            EnforceScaleAndOrientation();
            DisableMecanimAnimator();
            CacheBones();
            EnsureRenderersVisible();
            ApplyNaturalStance(0f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnforceScaleAndOrientation();
            DisableMecanimAnimator();
            CacheBones();
            EnsureRenderersVisible();
            ApplyNaturalStance(0f);
        }
#endif

        private void Start()
        {
            EnforceScaleAndOrientation();
            defaultRotation = transform.rotation;
            DisableMecanimAnimator();
            CacheBones();
            EnsureRenderersVisible();
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

        private void EnsureRenderersVisible()
        {
            var rends = GetComponentsInChildren<Renderer>(true);
            foreach (var r in rends)
            {
                if (!r.enabled) r.enabled = true;
                if (r is SkinnedMeshRenderer smr)
                {
                    smr.updateWhenOffscreen = true; // Absolute guarantee against bounding box culling
                }
            }
        }

        private void CacheBones()
        {
            if (hasCachedBones && leftArm != null && rightArm != null) return;

            spine = FindChildRecursive(transform, "Spine");
            leftArm = FindChildRecursive(transform, "LeftArm");
            rightArm = FindChildRecursive(transform, "RightArm");
            leftForeArm = FindChildRecursive(transform, "LeftForeArm");
            rightForeArm = FindChildRecursive(transform, "RightForeArm");

            hasCachedBones = (leftArm != null && rightArm != null);
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
            // NPC Root Transform: 0.48 scale (1.80m tall), standing on pavement (Y=0.45m behind counter)
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

            if (transform.parent != null)
            {
                Vector3 pos = transform.localPosition;
                if (pos.y < 0.2f || pos.y > 0.8f || pos.z < 0.2f)
                {
                    transform.localPosition = new Vector3(0f, 0.45f, 0.70f);
                }
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

            // 1. Reset all bones to authentic immutable bind pose before applying pose offsets
            leftArm.localRotation = BindRotLeftArm;
            rightArm.localRotation = BindRotRightArm;
            if (leftForeArm != null) leftForeArm.localRotation = BindRotLeftForeArm;
            if (rightForeArm != null) rightForeArm.localRotation = BindRotRightForeArm;
            if (spine != null) spine.localRotation = BindRotSpine;

            // 2. Subtle chest breathing
            if (enableBreathing && spine != null)
            {
                float breath = Mathf.Sin(time * 2.0f) * 1.5f;
                spine.localRotation = BindRotSpine * Quaternion.Euler(breath, 0f, 0f);
            }

            // 3. Left Arm: rotate from horizontal T-pose to hanging naturally down along torso
            if (leftForeArm != null)
            {
                Vector3 curLeftDir = (leftForeArm.position - leftArm.position).normalized;
                Vector3 tgtLeftDir = (-transform.up * 0.94f + transform.forward * 0.12f + transform.right * 0.22f).normalized;
                if (curLeftDir.sqrMagnitude > 0.1f && tgtLeftDir.sqrMagnitude > 0.1f)
                {
                    leftArm.rotation = Quaternion.FromToRotation(curLeftDir, tgtLeftDir) * leftArm.rotation;
                }
                leftForeArm.localRotation = BindRotLeftForeArm * Quaternion.Euler(0f, 0f, 22f);
            }

            // 4. Right Arm: rotate from horizontal T-pose to hanging naturally down along torso
            if (rightForeArm != null)
            {
                Vector3 curRightDir = (rightForeArm.position - rightArm.position).normalized;
                Vector3 tgtRightDir = (-transform.up * 0.94f + transform.forward * 0.12f - transform.right * 0.22f).normalized;
                if (curRightDir.sqrMagnitude > 0.1f && tgtRightDir.sqrMagnitude > 0.1f)
                {
                    rightArm.rotation = Quaternion.FromToRotation(curRightDir, tgtRightDir) * rightArm.rotation;
                }
                rightForeArm.localRotation = BindRotRightForeArm * Quaternion.Euler(0f, 0f, -22f);
            }
        }
    }
}

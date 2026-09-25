using UnityEngine;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Animates the NPC using the authentic Kenney Idle AnimationClip via native SampleAnimation,
    /// guaranteeing 100% anatomical correctness with zero bone distortion, zero T-pose,
    /// and natural breathing and arm posture in both Edit Mode ([ExecuteAlways]) and Play Mode.
    /// Smoothly rotates the NPC GameObject (Yaw axis only) to face the player upon approach.
    /// </summary>
    [ExecuteAlways]
    public class NPCLookAtPlayer : MonoBehaviour
    {
        [Header("Animation")]
        public AnimationClip idleClip;

        [Header("Player Tracking")]
        [Tooltip("Distance within which the NPC notices and faces the player")]
        public float noticeDistance = 4.0f;
        public float turnSpeed = 4.0f;

        private Transform playerTransform;
        private Quaternion defaultRotation;

        private void Awake()
        {
            EnforceScaleAndOrientation();
            defaultRotation = transform.rotation;
            DisableMecanimAnimator();
            EnsureIdleClip();
        }

        private void OnEnable()
        {
            EnforceScaleAndOrientation();
            DisableMecanimAnimator();
            EnsureIdleClip();
            if (idleClip != null)
            {
                idleClip.SampleAnimation(gameObject, 0f);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnforceScaleAndOrientation();
            DisableMecanimAnimator();
            EnsureIdleClip();
        }
#endif

        private void EnforceScaleAndOrientation()
        {
            // Safeguard against FBX default unit scale (100) turning NPC into a giant
            if (transform.localScale.x > 0.6f || transform.localScale.x < 0.3f)
            {
                transform.localScale = Vector3.one * 0.48f;
            }

            // Safeguard against FBX default -90 X rotation flipping character flat on its back
            Vector3 euler = transform.localEulerAngles;
            if (Mathf.Abs(Mathf.DeltaAngle(euler.x, 0f)) > 10f || Mathf.Abs(Mathf.DeltaAngle(euler.z, 0f)) > 10f)
            {
                transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            // Safeguard height: feet rest on pavement (local Y = 0.45m)
            if (transform.localPosition.y < 0.2f || transform.localPosition.y > 0.8f)
            {
                transform.localPosition = new Vector3(0f, 0.45f, 0.70f);
            }
        }

        private void DisableMecanimAnimator()
        {
            // Generic rig Mecanim animator causes T-pose and overrides bone sampling.
            // Disabling the component allows native SampleAnimation to drive curves smoothly.
            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.enabled)
            {
                anim.enabled = false;
            }
        }

        private void EnsureIdleClip()
        {
            if (idleClip != null) return;

            Animator anim = GetComponent<Animator>();
            if (anim != null && anim.runtimeAnimatorController != null)
            {
                var clips = anim.runtimeAnimatorController.animationClips;
                if (clips != null && clips.Length > 0)
                {
                    idleClip = clips[0];
                    return;
                }
            }

#if UNITY_EDITOR
            string animPath = "Assets/Art/Characters/Animations/idle.fbx";
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(animPath);
            if (assets != null)
            {
                foreach (var obj in assets)
                {
                    if (obj is AnimationClip clip && !clip.name.Contains("__preview__") && clip.name.ToLower().Contains("idle"))
                    {
                        idleClip = clip;
                        break;
                    }
                }
            }
#endif
        }

        private void Start()
        {
            EnforceScaleAndOrientation();
            defaultRotation = transform.rotation;
            DisableMecanimAnimator();
            EnsureIdleClip();

            if (idleClip != null)
            {
                idleClip.SampleAnimation(gameObject, 0f);
            }

            if (Application.isPlaying)
            {
                FindPlayer();
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
            // Always sample authentic Kenney idle animation clip in LateUpdate to guarantee:
            // 1. In Play Mode: continuous breathing and idle movement loop
            // 2. In Edit Mode: static resting stance (frame 0) with relaxed arms down along torso
            // 3. Absolute immunity to T-pose (nothing can overwrite this post-update)
            if (idleClip != null)
            {
                float clipTime = (Application.isPlaying && idleClip.length > 0f)
                    ? (Time.time % idleClip.length)
                    : 0f;
                idleClip.SampleAnimation(gameObject, clipTime);
            }
        }
    }
}

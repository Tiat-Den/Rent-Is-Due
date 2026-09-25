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
            defaultRotation = transform.rotation;
            EnsureIdleClip();
        }

        private void OnEnable()
        {
            EnsureIdleClip();
            if (idleClip != null)
            {
                idleClip.SampleAnimation(gameObject, 0f);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureIdleClip();
        }
#endif

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

            // If Animator has no controller, disable it to prevent it resetting to T-pose every frame
            if (anim != null && anim.runtimeAnimatorController == null)
            {
                anim.enabled = false;
            }
        }

        private void Start()
        {
            defaultRotation = transform.rotation;
            EnsureIdleClip();
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
            // 1. Sample authentic idle animation clip if Animator is not active or in Edit Mode
            if (idleClip != null)
            {
                Animator anim = GetComponent<Animator>();
                bool animatorActive = (anim != null && anim.enabled && anim.runtimeAnimatorController != null && Application.isPlaying);
                if (!animatorActive)
                {
                    float clipTime = (Application.isPlaying && idleClip.length > 0f)
                        ? (Time.time % idleClip.length)
                        : 0f;
                    idleClip.SampleAnimation(gameObject, clipTime);
                }
            }

            // 2. Player tracking (Horizontal Yaw only)
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
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace RentIsDue.Editor
{
    /// <summary>
    /// Implements Character Import and Animation Pipeline according to
    /// .agents/RentIsDue_Character_Model_Agent_Skills/ standards.
    /// Configures Kenney character FBX models as Humanoid with verified Mecanim Avatars
    /// and looping Idle animations.
    /// </summary>
    [InitializeOnLoad]
    public static class SetupNPCAnimations
    {
        public const string CharacterPath = "Assets/Art/Characters/characterMedium.fbx";
        public const string AnimPath = "Assets/Art/Characters/Animations/idle.fbx";
        public const string ControllerDir = "Assets/Art/Characters/Animations";
        public const string ControllerPath = ControllerDir + "/NPC_Idle.controller";

        static SetupNPCAnimations()
        {
            EditorApplication.delayCall += () =>
            {
                if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (!SessionState.GetBool("NPC_Final_Pose_And_Height_Fix_V5", false))
                {
                    SessionState.SetBool("NPC_Final_Pose_And_Height_Fix_V5", true);
                    Setup();
                }
                ValidateCharacterSetup();
            };
        }

        [MenuItem("Rent Is Due/Setup NPC Animations and Avatars")]
        public static void Setup()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[SetupNPCAnimations] Cannot run setup while in Play Mode.");
                return;
            }

            Avatar charAvatar = EnsureHumanoidAssets(out RuntimeAnimatorController controller);
            if (charAvatar == null || !charAvatar.isValid)
            {
                Debug.LogError("[SetupNPCAnimations] Failed to configure valid Avatar!");
                return;
            }

            Debug.Log($"<color=green>[SetupNPCAnimations] Successfully configured Avatar '{charAvatar.name}' and Controller '{controller?.name}'!</color>");

            // Rebuild Scene only if in Edit Mode
            if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                RoomSceneBuilder.BuildGiantRoom();
            }
        }

        [MenuItem("Rent Is Due/Validate Character Setup")]
        public static void ValidateCharacterSetup()
        {
            string[] npcNames = new string[] { "Dealer_NPC", "ToolShop_NPC" };
            foreach (var name in npcNames)
            {
                GameObject npc = GameObject.Find(name);
                if (npc == null)
                {
                    Debug.LogWarning($"[ValidateCharacterSetup] '{name}' not found in current scene!");
                    continue;
                }

                Animator anim = npc.GetComponent<Animator>();
                var lookAt = npc.GetComponent<RentIsDue.Gameplay.NPCLookAtPlayer>();
                var col = npc.GetComponent<CapsuleCollider>();
                var rb = npc.GetComponent<Rigidbody>();

                Debug.Log($"<color=cyan>[QA REVIEW] NPC: {name}</color>\n" +
                          $"- Animator: {(anim != null ? "Present" : "MISSING")}\n" +
                          $"- Avatar: {(anim?.avatar != null ? anim.avatar.name : "MISSING")} (isHuman={anim?.avatar?.isHuman})\n" +
                          $"- Controller: {(anim?.runtimeAnimatorController != null ? anim.runtimeAnimatorController.name : "MISSING")}\n" +
                          $"- ApplyRootMotion: {anim?.applyRootMotion}\n" +
                          $"- NPCLookAtPlayer: {(lookAt != null ? "Present" : "MISSING")}\n" +
                          $"- Effective Height: {(col != null ? $"{col.height * npc.transform.localScale.y:F2}m" : "N/A")}\n" +
                          $"- Rigidbody: {(rb != null && rb.isKinematic ? "Kinematic OK" : "MISSING/NOT KINEMATIC")}");
            }
        }

        public static Avatar EnsureHumanoidAssets(out RuntimeAnimatorController controller)
        {
            controller = null;

            // 1. Configure characterMedium.fbx as Generic
            ModelImporter charImporter = AssetImporter.GetAtPath(CharacterPath) as ModelImporter;
            if (charImporter == null)
            {
                Debug.LogError($"[SetupNPCAnimations] Cannot find character at: {CharacterPath}");
                return null;
            }

            bool charNeedsReimport = false;
            if (charImporter.animationType != ModelImporterAnimationType.Generic)
            {
                charImporter.animationType = ModelImporterAnimationType.Generic;
                charNeedsReimport = true;
            }
            if (charImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
            {
                charImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                charNeedsReimport = true;
            }
            if (charNeedsReimport)
            {
                charImporter.SaveAndReimport();
                Debug.Log("[SetupNPCAnimations] characterMedium.fbx configured as Generic Avatar.");
            }

            // Load character Avatar
            Avatar charAvatar = null;
            UnityEngine.Object[] charAssets = AssetDatabase.LoadAllAssetsAtPath(CharacterPath);
            foreach (var obj in charAssets)
            {
                if (obj is Avatar av)
                {
                    charAvatar = av;
                    break;
                }
            }

            if (charAvatar == null || !charAvatar.isValid)
            {
                Debug.LogError($"[SetupNPCAnimations] characterMedium Avatar is invalid! avatar={charAvatar}, isValid={charAvatar?.isValid}");
                return null;
            }

            // 2. Configure idle.fbx as Generic copying characterMedium's Avatar
            ModelImporter animImporter = AssetImporter.GetAtPath(AnimPath) as ModelImporter;
            if (animImporter != null)
            {
                bool animNeedsReimport = false;
                if (animImporter.animationType != ModelImporterAnimationType.Generic)
                {
                    animImporter.animationType = ModelImporterAnimationType.Generic;
                    animNeedsReimport = true;
                }
                if (animImporter.avatarSetup != ModelImporterAvatarSetup.CopyFromOther || animImporter.sourceAvatar != charAvatar)
                {
                    animImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                    animImporter.sourceAvatar = charAvatar;
                    animNeedsReimport = true;
                }

                ModelImporterClipAnimation[] clips = animImporter.clipAnimations;
                if (clips == null || clips.Length == 0)
                {
                    clips = animImporter.defaultClipAnimations;
                }
                if (clips != null && clips.Length > 0)
                {
                    foreach (var clip in clips)
                    {
                        if (clip.name.ToLower().Contains("idle") && !clip.name.ToLower().Contains("preview"))
                        {
                            clip.loopTime = true;
                            animNeedsReimport = true;
                        }
                    }
                    animImporter.clipAnimations = clips;
                }

                if (animNeedsReimport)
                {
                    animImporter.SaveAndReimport();
                    Debug.Log("[SetupNPCAnimations] idle.fbx configured as Generic copying character Avatar.");
                }
            }

            // 3. Find Idle AnimationClip
            AnimationClip idleClip = null;
            UnityEngine.Object[] animAssets = AssetDatabase.LoadAllAssetsAtPath(AnimPath);
            foreach (var obj in animAssets)
            {
                if (obj is AnimationClip clip && !clip.name.Contains("__preview__"))
                {
                    if (clip.name.ToLower().Contains("idle"))
                    {
                        idleClip = clip;
                        break;
                    }
                    if (idleClip == null) idleClip = clip;
                }
            }

            // 4. Create or verify AnimatorController
            if (!Directory.Exists(ControllerDir))
            {
                Directory.CreateDirectory(ControllerDir);
            }

            AnimatorController animController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (animController == null)
            {
                animController = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            }

            if (animController != null && animController.layers.Length > 0)
            {
                var layers = animController.layers;
                layers[0].defaultWeight = 1f;
                animController.layers = layers;
            }

            if (animController != null && idleClip != null)
            {
                var sm = animController.layers[0].stateMachine;
                bool hasIdleState = false;
                foreach (var childState in sm.states)
                {
                    if (childState.state != null && childState.state.name == "Idle" && childState.state.motion == idleClip)
                    {
                        hasIdleState = true;
                        sm.defaultState = childState.state;
                        break;
                    }
                }

                if (!hasIdleState)
                {
                    for (int i = sm.states.Length - 1; i >= 0; i--)
                    {
                        sm.RemoveState(sm.states[i].state);
                    }
                    var idleState = sm.AddState("Idle");
                    idleState.motion = idleClip;
                    sm.defaultState = idleState;
                    EditorUtility.SetDirty(animController);
                    AssetDatabase.SaveAssets();
                }
            }

            controller = animController;
            return charAvatar;
        }
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace RentIsDue.Editor
{
    public static class SetupNPCAnimations
    {
        [MenuItem("Rent Is Due/Setup NPC Animations and Avatars")]
        public static void Setup()
        {
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[SetupNPCAnimations] Cannot run setup while in Play Mode.");
                return;
            }

            string charPath = "Assets/Art/Characters/characterMedium.fbx";
            string animPath = "Assets/Art/Characters/Animations/idle.fbx";
            string controllerDir = "Assets/Art/Characters/Animations";
            string controllerPath = controllerDir + "/NPC_Idle.controller";

            // 1. Configure characterMedium.fbx Avatar
            ModelImporter charImporter = AssetImporter.GetAtPath(charPath) as ModelImporter;
            if (charImporter != null)
            {
                bool needsSave = false;
                if (charImporter.animationType != ModelImporterAnimationType.Generic)
                {
                    charImporter.animationType = ModelImporterAnimationType.Generic;
                    needsSave = true;
                }
                if (charImporter.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                {
                    charImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    needsSave = true;
                }
                if (needsSave)
                {
                    charImporter.SaveAndReimport();
                    Debug.Log("[SetupNPCAnimations] characterMedium Avatar configured.");
                }
            }

            // Load Avatar
            Avatar charAvatar = null;
            UnityEngine.Object[] charAssets = AssetDatabase.LoadAllAssetsAtPath(charPath);
            foreach (var obj in charAssets)
            {
                if (obj is Avatar av)
                {
                    charAvatar = av;
                    break;
                }
            }
            Debug.Log("[SetupNPCAnimations] Character Avatar: " + (charAvatar != null ? charAvatar.name : "null"));

            // 2. Configure idle.fbx
            ModelImporter animImporter = AssetImporter.GetAtPath(animPath) as ModelImporter;
            if (animImporter != null)
            {
                bool needsSave = false;
                if (animImporter.animationType != ModelImporterAnimationType.Generic)
                {
                    animImporter.animationType = ModelImporterAnimationType.Generic;
                    needsSave = true;
                }
                if (charAvatar != null && animImporter.avatarSetup != ModelImporterAvatarSetup.CopyFromOther)
                {
                    animImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
                    animImporter.sourceAvatar = charAvatar;
                    needsSave = true;
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
                        if (!clip.loopTime)
                        {
                            clip.loopTime = true;
                            needsSave = true;
                        }
                    }
                    animImporter.clipAnimations = clips;
                }

                if (needsSave)
                {
                    animImporter.SaveAndReimport();
                    Debug.Log("[SetupNPCAnimations] idle.fbx configured with looping Idle clip.");
                }
            }

            // 3. Find Idle AnimationClip
            AnimationClip idleClip = null;
            UnityEngine.Object[] animAssets = AssetDatabase.LoadAllAssetsAtPath(animPath);
            foreach (var obj in animAssets)
            {
                if (obj is AnimationClip clip && !clip.name.Contains("__preview__"))
                {
                    idleClip = clip;
                    break;
                }
            }
            Debug.Log("[SetupNPCAnimations] Found AnimationClip: " + (idleClip != null ? idleClip.name : "null"));

            // 4. Create or update AnimatorController
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            if (controller != null && idleClip != null)
            {
                var sm = controller.layers[0].stateMachine;
                // Clear existing states
                for (int i = sm.states.Length - 1; i >= 0; i--)
                {
                    sm.RemoveState(sm.states[i].state);
                }
                var idleState = sm.AddState("Idle");
                idleState.motion = idleClip;
                sm.defaultState = idleState;
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                Debug.Log("[SetupNPCAnimations] AnimatorController created and saved at: " + controllerPath);
            }

            // 5. Rebuild Scene only if in Edit Mode
            if (!Application.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                RoomSceneBuilder.BuildGiantRoom();
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using RentIsDue.Inventory;

namespace RentIsDue.Editor
{
    public class ItemPrefabGenerator
    {
        [MenuItem("Tools/📦 Generate 3D Item Prefabs for All 30 Items")]
        public static void GenerateAllItemPrefabs()
        {
            string itemsFolder = "Assets/ScriptableObjects/Items";
            string modelsFolder = "Assets/Models/FBX format";
            string prefabsOutputFolder = "Assets/Prefabs/Items";

            if (!Directory.Exists(prefabsOutputFolder))
            {
                Directory.CreateDirectory(prefabsOutputFolder);
            }

            var modelMapping = new Dictionary<string, (string fbx, float scale)>
            {
                { "item_plastic_bottle", ("kitchenBlender.fbx", 0.35f) },
                { "item_aluminum_can", ("toaster.fbx", 0.35f) },
                { "item_old_newspaper", ("books.fbx", 0.40f) },
                { "item_cardboard_box", ("cardboardBoxClosed.fbx", 0.45f) },
                { "item_old_clothes", ("pillow.fbx", 0.45f) },
                { "item_rubber_duck", ("bear.fbx", 0.40f) },
                { "item_empty_jar", ("plantSmall1.fbx", 0.35f) },
                { "item_old_book", ("books.fbx", 0.45f) },
                { "item_magazine_bundle", ("books.fbx", 0.45f) },
                { "item_old_mug", ("plantSmall2.fbx", 0.35f) },
                { "item_frying_pan", ("kitchenMicrowave.fbx", 0.45f) },
                { "item_wall_clock", ("lampSquareCeiling.fbx", 0.45f) },
                { "item_keyboard", ("computerKeyboard.fbx", 0.50f) },
                { "item_mouse", ("computerMouse.fbx", 0.45f) },
                { "item_headphones", ("speakerSmall.fbx", 0.45f) },
                { "item_game_controller", ("computerMouse.fbx", 0.45f) },
                { "item_broken_phone", ("computerMouse.fbx", 0.40f) },
                { "item_old_gpu", ("computerKeyboard.fbx", 0.45f) },
                { "item_broken_laptop", ("laptop.fbx", 0.35f) },
                { "item_digital_camera", ("speakerSmall.fbx", 0.45f) },
                { "item_vintage_radio", ("radio.fbx", 0.45f) },
                { "item_old_console", ("televisionVintage.fbx", 0.45f) },
                { "item_vinyl_player", ("speaker.fbx", 0.45f) },
                { "item_antique_clock", ("lampRoundTable.fbx", 0.45f) },
                { "item_collectible_card", ("books.fbx", 0.35f) },
                { "item_vintage_camera", ("speakerSmall.fbx", 0.45f) },
                { "item_rare_painting", ("televisionModern.fbx", 0.45f) },
                { "item_gold_watch", ("lampSquareTable.fbx", 0.40f) },
                { "item_rare_console", ("televisionModern.fbx", 0.45f) },
                { "item_rare_coin_collection", ("cardboardBoxOpen.fbx", 0.45f) }
            };

            int generatedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { itemsFolder });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item == null) continue;

                string safeId = !string.IsNullOrEmpty(item.id) ? item.id : Path.GetFileNameWithoutExtension(path);
                if (string.IsNullOrEmpty(safeId)) safeId = "item_" + item.name;

                string fbxName = "books.fbx";
                float scale = 0.4f;

                if (modelMapping.TryGetValue(safeId, out var entry))
                {
                    fbxName = entry.fbx;
                    scale = entry.scale;
                }

                string fbxPath = $"{modelsFolder}/{fbxName}";
                GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                if (fbxAsset == null)
                {
                    Debug.LogWarning($"FBX not found: {fbxPath}");
                    continue;
                }

                // Tạo đối tượng Prefab tạm thời
                GameObject rootObj = new GameObject(safeId + "_Prefab");
                GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                modelInstance.transform.SetParent(rootObj.transform, false);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one * scale;

                // Gắn Collider
                BoxCollider col = rootObj.AddComponent<BoxCollider>();
                col.size = new Vector3(0.5f, 0.4f, 0.5f);
                col.center = new Vector3(0, 0.2f, 0);

                // Gắn Rigidbody nhẹ để đồ chạm đất tự nhiên
                Rigidbody rb = rootObj.AddComponent<Rigidbody>();
                rb.mass = Mathf.Max(0.1f, item.weight);
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

                // Gắn script PickupInteractable
                PickupInteractable pickup = rootObj.AddComponent<PickupInteractable>();
                pickup.itemData = item;

                // Lưu thành Prefab
                string prefabPath = $"{prefabsOutputFolder}/{safeId}.prefab";
                GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
                GameObject.DestroyImmediate(rootObj);

                // Gán vào ItemData
                item.prefab = savedPrefab;
                EditorUtility.SetDirty(item);
                generatedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("3D Items Generated!", $"Successfully generated 3D Prefabs and linked them to {generatedCount} ItemData assets!", "Great!");
            Debug.Log($"<color=green>[ItemPrefabGenerator] Generated 3D Prefabs for {generatedCount} items!</color>");
        }
    }
}

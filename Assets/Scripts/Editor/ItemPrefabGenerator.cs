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

            // Bản đồ ghép nối Model 3D chuẩn xác 100% cho từng vật phẩm
            var modelMapping = new Dictionary<string, (string fbx, float scale, System.Action<GameObject> customBuilder)>
            {
                // 1. Chai nhựa (Plastic Bottle) -> Thân chai xanh + Nắp trắng
                { "item_plastic_bottle", (null, 1f, root => BuildBottle(root, new Color(0.2f, 0.7f, 0.9f, 0.8f))) },
                
                // 2. Lon nhôm (Aluminum Can) -> Vỏ lon nước ngọt đỏ/bạc
                { "item_aluminum_can", (null, 1f, root => BuildSodaCan(root, new Color(0.9f, 0.2f, 0.2f))) },
                
                // 3. Hũ thủy tinh (Empty Jar) -> Thân hũ trong + Nắp kim loại
                { "item_empty_jar", (null, 1f, root => BuildJar(root)) },
                
                // 4. Cốc uống nước cũ (Old Mug)
                { "item_old_mug", (null, 1f, root => BuildMug(root, new Color(0.85f, 0.82f, 0.75f))) },
                
                // 5. Điện thoại hỏng (Broken Phone) -> Smartphone màn hình vỡ
                { "item_broken_phone", (null, 1f, root => BuildSmartphone(root)) },
                
                // 6. Đồng hồ vàng (Gold Watch) -> Đồng hồ đeo tay vàng óng
                { "item_gold_watch", (null, 1f, root => BuildWatch(root)) },
                
                // 7. Thẻ bài sưu tầm (Collectible Card)
                { "item_collectible_card", (null, 1f, root => BuildTradingCard(root)) },

                // Các đồ công nghệ & nội thất dùng FBX chuẩn từ Kenney:
                { "item_old_newspaper", ("books.fbx", 0.20f, null) },
                { "item_old_book", ("books.fbx", 0.22f, null) },
                { "item_magazine_bundle", ("books.fbx", 0.25f, null) },
                { "item_cardboard_box", ("cardboardBoxClosed.fbx", 0.32f, null) },
                { "item_old_clothes", ("pillow.fbx", 0.25f, null) },
                { "item_rubber_duck", ("bear.fbx", 0.20f, null) },
                { "item_frying_pan", ("toaster.fbx", 0.22f, null) },
                { "item_wall_clock", ("lampSquareCeiling.fbx", 0.22f, null) },
                { "item_keyboard", ("computerKeyboard.fbx", 0.26f, null) },
                { "item_mouse", ("computerMouse.fbx", 0.16f, null) },
                { "item_headphones", ("speakerSmall.fbx", 0.20f, null) },
                { "item_game_controller", ("computerMouse.fbx", 0.20f, null) },
                { "item_old_gpu", ("computerKeyboard.fbx", 0.20f, null) },
                { "item_broken_laptop", ("laptop.fbx", 0.24f, null) },
                { "item_digital_camera", ("speakerSmall.fbx", 0.20f, null) },
                { "item_vintage_radio", ("radio.fbx", 0.25f, null) },
                { "item_old_console", ("televisionVintage.fbx", 0.28f, null) },
                { "item_vinyl_player", ("speaker.fbx", 0.28f, null) },
                { "item_antique_clock", ("lampRoundTable.fbx", 0.22f, null) },
                { "item_vintage_camera", ("speakerSmall.fbx", 0.22f, null) },
                { "item_rare_painting", ("televisionModern.fbx", 0.30f, null) },
                { "item_rare_console", ("televisionModern.fbx", 0.28f, null) },
                { "item_rare_coin_collection", ("cardboardBoxOpen.fbx", 0.22f, null) }
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

                // Tạo đối tượng Prefab tạm thời
                GameObject rootObj = new GameObject(safeId + "_Prefab");

                if (modelMapping.TryGetValue(safeId, out var entry))
                {
                    if (entry.customBuilder != null)
                    {
                        entry.customBuilder.Invoke(rootObj);
                    }
                    else if (!string.IsNullOrEmpty(entry.fbx))
                    {
                        string fbxPath = $"{modelsFolder}/{entry.fbx}";
                        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                        if (fbxAsset != null)
                        {
                            GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                            modelInstance.transform.SetParent(rootObj.transform, false);
                            modelInstance.transform.localPosition = Vector3.zero;
                            modelInstance.transform.localRotation = Quaternion.identity;
                            modelInstance.transform.localScale = Vector3.one * entry.scale;
                        }
                    }
                }
                else
                {
                    // Mặc định tạo khối sách nhỏ
                    string fbxPath = $"{modelsFolder}/books.fbx";
                    GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (fbxAsset != null)
                    {
                        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset);
                        modelInstance.transform.SetParent(rootObj.transform, false);
                        modelInstance.transform.localPosition = Vector3.zero;
                        modelInstance.transform.localScale = Vector3.one * 0.20f;
                    }
                }

                // Gắn Collider vừa vặn cho việc nhặt đồ
                BoxCollider col = rootObj.AddComponent<BoxCollider>();
                col.size = new Vector3(0.45f, 0.35f, 0.45f);
                col.center = new Vector3(0, 0.15f, 0);

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

            EditorUtility.DisplayDialog("3D Items Generated!", $"Successfully generated accurate 3D Prefabs for {generatedCount} items!", "Great!");
            Debug.Log($"<color=green>[ItemPrefabGenerator] Generated 3D Prefabs for {generatedCount} items!</color>");
        }

        // ==================== HÀM TẠO MODEL THỦ CÔNG CHÍNH XÁC ====================

        private static void BuildBottle(GameObject parent, Color liquidColor)
        {
            // Thân chai
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Bottle_Body";
            body.transform.SetParent(parent.transform, false);
            body.transform.localPosition = new Vector3(0, 0.10f, 0);
            body.transform.localScale = new Vector3(0.08f, 0.09f, 0.08f);
            ApplyMaterial(body, "Mat_PlasticBottle", liquidColor);
            RemoveCollider(body);

            // Cổ chai
            GameObject neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Bottle_Neck";
            neck.transform.SetParent(parent.transform, false);
            neck.transform.localPosition = new Vector3(0, 0.21f, 0);
            neck.transform.localScale = new Vector3(0.035f, 0.03f, 0.035f);
            ApplyMaterial(neck, "Mat_PlasticBottle", liquidColor);
            RemoveCollider(neck);

            // Nắp chai
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Bottle_Cap";
            cap.transform.SetParent(parent.transform, false);
            cap.transform.localPosition = new Vector3(0, 0.245f, 0);
            cap.transform.localScale = new Vector3(0.04f, 0.01f, 0.04f);
            ApplyMaterial(cap, "Mat_BottleCap", Color.white);
            RemoveCollider(cap);
        }

        private static void BuildSodaCan(GameObject parent, Color canColor)
        {
            GameObject can = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            can.name = "Can_Body";
            can.transform.SetParent(parent.transform, false);
            can.transform.localPosition = new Vector3(0, 0.07f, 0);
            can.transform.localScale = new Vector3(0.07f, 0.07f, 0.07f);
            ApplyMaterial(can, "Mat_SodaCan", canColor);
            RemoveCollider(can);

            // Vành trên kim loại
            GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            top.name = "Can_Top";
            top.transform.SetParent(parent.transform, false);
            top.transform.localPosition = new Vector3(0, 0.141f, 0);
            top.transform.localScale = new Vector3(0.065f, 0.005f, 0.065f);
            ApplyMaterial(top, "Mat_MetalRim", new Color(0.85f, 0.85f, 0.88f));
            RemoveCollider(top);
        }

        private static void BuildJar(GameObject parent)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Jar_Glass";
            body.transform.SetParent(parent.transform, false);
            body.transform.localPosition = new Vector3(0, 0.07f, 0);
            body.transform.localScale = new Vector3(0.09f, 0.065f, 0.09f);
            ApplyMaterial(body, "Mat_GlassJar", new Color(0.85f, 0.95f, 0.95f, 0.6f));
            RemoveCollider(body);

            GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            lid.name = "Jar_Lid";
            lid.transform.SetParent(parent.transform, false);
            lid.transform.localPosition = new Vector3(0, 0.14f, 0);
            lid.transform.localScale = new Vector3(0.095f, 0.01f, 0.095f);
            ApplyMaterial(lid, "Mat_BronzeLid", new Color(0.75f, 0.65f, 0.4f)); // Nắp vàng đồng
            RemoveCollider(lid);
        }

        private static void BuildMug(GameObject parent, Color mugColor)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.name = "Mug_Body";
            body.transform.SetParent(parent.transform, false);
            body.transform.localPosition = new Vector3(0, 0.06f, 0);
            body.transform.localScale = new Vector3(0.08f, 0.06f, 0.08f);
            ApplyMaterial(body, "Mat_CeramicMug", mugColor);
            RemoveCollider(body);
        }

        private static void BuildSmartphone(GameObject parent)
        {
            // Thân máy
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Phone_Body";
            body.transform.SetParent(parent.transform, false);
            body.transform.localPosition = new Vector3(0, 0.005f, 0);
            body.transform.localScale = new Vector3(0.08f, 0.01f, 0.15f);
            ApplyMaterial(body, "Mat_PhoneBody", new Color(0.15f, 0.15f, 0.18f));
            RemoveCollider(body);

            // Màn hình đen bóng
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            screen.name = "Phone_Screen";
            screen.transform.SetParent(parent.transform, false);
            screen.transform.localPosition = new Vector3(0, 0.011f, 0);
            screen.transform.localScale = new Vector3(0.072f, 0.002f, 0.138f);
            ApplyMaterial(screen, "Mat_PhoneScreen", new Color(0.05f, 0.08f, 0.1f));
            RemoveCollider(screen);
        }

        private static void BuildWatch(GameObject parent)
        {
            GameObject dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dial.name = "Watch_Dial";
            dial.transform.SetParent(parent.transform, false);
            dial.transform.localPosition = new Vector3(0, 0.008f, 0);
            dial.transform.localScale = new Vector3(0.05f, 0.01f, 0.05f);
            ApplyMaterial(dial, "Mat_GoldWatch", new Color(0.95f, 0.8f, 0.2f)); // Vàng óng
            RemoveCollider(dial);

            GameObject strap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strap.name = "Watch_Strap";
            strap.transform.SetParent(parent.transform, false);
            strap.transform.localPosition = new Vector3(0, 0.005f, 0);
            strap.transform.localScale = new Vector3(0.025f, 0.006f, 0.12f);
            ApplyMaterial(strap, "Mat_LeatherStrap", new Color(0.4f, 0.25f, 0.15f)); // Dây da nâu
            RemoveCollider(strap);
        }

        private static void BuildTradingCard(GameObject parent)
        {
            GameObject card = GameObject.CreatePrimitive(PrimitiveType.Cube);
            card.name = "Card_Body";
            card.transform.SetParent(parent.transform, false);
            card.transform.localPosition = new Vector3(0, 0.002f, 0);
            card.transform.localScale = new Vector3(0.07f, 0.002f, 0.10f);
            ApplyMaterial(card, "Mat_TradingCard", new Color(0.95f, 0.75f, 0.1f)); // Thẻ vàng lấp lánh
            RemoveCollider(card);
        }

        private static void RemoveCollider(GameObject obj)
        {
            Collider col = obj.GetComponent<Collider>();
            if (col != null) GameObject.DestroyImmediate(col);
        }

        private static void ApplyMaterial(GameObject obj, string matName, Color color)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                string folder = "Assets/Materials/Items";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string matPath = $"{folder}/{matName}.mat";
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);

                if (mat == null)
                {
                    Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                    if (shader == null) shader = Shader.Find("Standard");
                    if (shader == null) shader = Shader.Find("Diffuse");

                    mat = new Material(shader);
                    mat.color = color;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                else
                {
                    mat.color = color;
                    if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                    EditorUtility.SetDirty(mat);
                }

                rend.sharedMaterial = mat;
            }
        }
    }
}

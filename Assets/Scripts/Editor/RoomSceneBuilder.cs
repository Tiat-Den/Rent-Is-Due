using System.IO;
using UnityEngine;
using UnityEditor;
using RentIsDue.Core;
using RentIsDue.Economy;
using RentIsDue.Inventory;
using RentIsDue.Loot;
using RentIsDue.Shop;

namespace RentIsDue.Editor
{
    public class RoomSceneBuilder
    {
        [MenuItem("Tools/🏠 Build Spacious Room (15m x 12m)")]
        public static void BuildSpaciousRoom()
        {
            BuildRoomInternal("Spacious Room (15m x 12m)", 15f, 12f, 3.8f);
        }

        [MenuItem("Tools/🏠 Build Tiny Room (8x6m - Chuẩn MVP)")]
        public static void BuildTinyRoom()
        {
            BuildRoomInternal("Tiny Room (8x6m)", 8f, 6f, 2.6f);
        }

        private static void BuildRoomInternal(string roomTitle, float roomWidth, float roomDepth, float wallHeight)
        {
            string modelsFolder = "Assets/Models/FBX format";

            GameObject roomRoot = GameObject.Find("TinyRoom_Environment");
            if (roomRoot != null)
            {
                if (EditorUtility.DisplayDialog("Rebuild Room", $"{roomRoot.name} already exists. Do you want to replace it?", "Yes, Rebuild", "Cancel"))
                {
                    Undo.DestroyObjectImmediate(roomRoot);
                }
                else
                {
                    return;
                }
            }

            roomRoot = new GameObject("TinyRoom_Environment");
            roomRoot.transform.position = Vector3.zero;
            roomRoot.transform.rotation = Quaternion.identity;
            roomRoot.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(roomRoot, $"Build {roomTitle}");

            // 1. Tạo Vỏ Phòng (Sàn gỗ, 4 Tường màu kem, Trần nhà)
            CreateRoomShell(roomRoot, roomWidth, roomDepth, wallHeight);

            float halfW = roomWidth / 2f;
            float halfD = roomDepth / 2f;

            // 2. 🛏️ GÓC GIƯỜNG NGỦ (Góc Tây Bắc - Tọa độ: X = -5.8, Z = +4.5)
            GameObject bed = SpawnModel(modelsFolder, "bedSingle.fbx", new Vector3(-halfW + 1.8f, 0, halfD - 1.6f), Quaternion.Euler(0, 90, 0), roomRoot, 0.9f);
            SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-halfW + 1.8f, 0, halfD - 3.2f), Quaternion.Euler(0, 90, 0), roomRoot, 0.9f);
            SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-halfW + 0.8f, 0, halfD - 3.2f), Quaternion.identity, roomRoot, 0.9f);

            // 3. 💻 BÀN LÀM VIỆC & NÂNG CẤP PC (Phía Bắc - Cách Giường 7 mét! Tọa độ: X = +1.5, Z = +5.0)
            GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(1.5f, 0, halfD - 1.0f), Quaternion.Euler(0, 180, 0), roomRoot, 0.9f);
            SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(1.5f, 0, halfD - 2.2f), Quaternion.identity, roomRoot, 0.9f);
            SpawnModel(modelsFolder, "computerScreen.fbx", new Vector3(1.1f, 0.70f, halfD - 1.0f), Quaternion.Euler(0, 180, 0), roomRoot, 0.9f);
            
            // Laptop mở cửa hàng Nâng Cấp
            GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(1.9f, 0.70f, halfD - 1.0f), Quaternion.Euler(0, 160, 0), roomRoot, 0.9f);
            if (laptop != null)
            {
                EnsureCollider(laptop);
                laptop.AddComponent<UpgradeInteractable>();
            }

            // Gắn Searchable cho Bàn làm việc
            if (desk != null)
            {
                EnsureCollider(desk);
                SearchableObject deskSearch = desk.AddComponent<SearchableObject>();
                deskSearch.containerName = "Study Desk";
                deskSearch.searchDuration = 2.5f;
                deskSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/DeskLootTable.asset");
            }

            // 4. 🗑️ GÓC THÙNG RÁC (Góc Đông Bắc - Tọa độ: X = +6.2, Z = +4.8)
            GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(halfW - 1.2f, 0, halfD - 1.2f), Quaternion.Euler(0, -35, 0), roomRoot, 0.9f);
            if (trash != null)
            {
                EnsureCollider(trash);
                SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                trashSearch.containerName = "Trash Pile";
                trashSearch.searchDuration = 1.5f;
                trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
            }

            // 5. 🍳 KHU BẾP (Mép tường phía Đông - Tọa độ: X = +6.5, Z = 0.0)
            GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(halfW - 1.0f, 0, 0.5f), Quaternion.Euler(0, -90, 0), roomRoot, 0.9f);
            SpawnModel(modelsFolder, "kitchenSink.fbx", new Vector3(halfW - 1.0f, 0, -1.0f), Quaternion.Euler(0, -90, 0), roomRoot, 0.9f);
            if (kitchen != null)
            {
                EnsureCollider(kitchen);
                SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                kitchenSearch.containerName = "Kitchen Cabinet";
                kitchenSearch.searchDuration = 2.0f;
                kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
            }

            // 6. 🚪 TỦ QUẦN ÁO & GIÁ SÁCH (Mép tường phía Tây - Tọa độ: X = -6.5, Z = -0.5)
            GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-halfW + 1.0f, 0, -0.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.9f);
            SpawnModel(modelsFolder, "bookcaseOpen.fbx", new Vector3(-halfW + 1.0f, 0, -2.0f), Quaternion.Euler(0, 90, 0), roomRoot, 0.9f);
            if (wardrobe != null)
            {
                EnsureCollider(wardrobe);
                SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                wardrobeSearch.containerName = "Wardrobe";
                wardrobeSearch.searchDuration = 3.0f;
                wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
            }

            // 7. 🔒 KÉT SẮT BÍ MẬT (Góc Tây Nam - Tọa độ: X = -6.2, Z = -4.8)
            GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-halfW + 1.2f, 0, -halfD + 1.2f), Quaternion.Euler(0, 45, 0), roomRoot, 0.9f);
            if (safe != null)
            {
                EnsureCollider(safe);
                SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                safeSearch.containerName = "Secret Safe";
                safeSearch.searchDuration = 4.0f;
                safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
            }

            // 8. 👤 QUẦY DEALER VE CHAI (Góc Đông Nam - Tọa độ: X = +5.8, Z = -4.5)
            GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(halfW - 1.6f, 0, -halfD + 1.5f), Quaternion.Euler(0, -45, 0), roomRoot, 0.9f);
            if (dealerDesk != null)
            {
                EnsureCollider(dealerDesk);
                dealerDesk.AddComponent<DealerInteractable>();
            }

            // 9. Thiết lập ánh sáng phòng ấm cúng
            SetupLighting(roomRoot, wallHeight);

            // 10. Đặt Player vào tâm phòng (Tọa độ 0, 1, 0)
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1.0f, 0);
            }

            EditorUtility.DisplayDialog($"{roomTitle} Built!", $"Successfully built {roomTitle} with wide open layout (4m to 9m separation between furniture)!", "Great!");
            Debug.Log($"<color=green>[RoomSceneBuilder] Successfully built {roomTitle}!</color>");
        }

        private static void CreateRoomShell(GameObject parent, float width, float depth, float height)
        {
            float halfW = width / 2f;
            float halfD = depth / 2f;
            float halfH = height / 2f;

            // Sàn nhà (Floor)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Room_Floor";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);
            ApplyMaterial(floor, new Color(0.35f, 0.28f, 0.22f)); // Sàn gỗ nâu

            // 4 Bức tường bao quanh
            CreateWall("Wall_North", new Vector3(0, halfH, halfD), new Vector3(width, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            CreateWall("Wall_South", new Vector3(0, halfH, -halfD), new Vector3(width, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            CreateWall("Wall_West", new Vector3(-halfW, halfH, 0), new Vector3(0.3f, height, depth), parent, new Color(0.82f, 0.80f, 0.76f));
            CreateWall("Wall_East", new Vector3(halfW, halfH, 0), new Vector3(0.3f, height, depth), parent, new Color(0.82f, 0.80f, 0.76f));

            // Trần nhà (Ceiling)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Room_Ceiling";
            ceiling.transform.SetParent(parent.transform, false);
            ceiling.transform.localPosition = new Vector3(0, height, 0);
            ceiling.transform.localRotation = Quaternion.Euler(180, 0, 0);
            ceiling.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);
            ApplyMaterial(ceiling, new Color(0.92f, 0.92f, 0.92f));
        }

        private static void CreateWall(string name, Vector3 pos, Vector3 size, GameObject parent, Color wallColor)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform, false);
            wall.transform.localPosition = pos;
            wall.transform.localScale = size;
            ApplyMaterial(wall, wallColor);
        }

        private static void ApplyMaterial(GameObject obj, Color color)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Diffuse");

                Material mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                rend.sharedMaterial = mat;
            }
        }

        private static GameObject SpawnModel(string folder, string fileName, Vector3 pos, Quaternion rot, GameObject parent, float scale = 1f)
        {
            string path = $"{folder}/{fileName}";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[RoomSceneBuilder] Model not found at: {path}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = Path.GetFileNameWithoutExtension(fileName);
            instance.transform.SetParent(parent.transform, false);
            instance.transform.localPosition = pos;
            instance.transform.localRotation = rot;
            instance.transform.localScale = Vector3.one * scale;

            EnsureCollider(instance);
            return instance;
        }

        private static void EnsureCollider(GameObject obj)
        {
            Collider[] cols = obj.GetComponentsInChildren<Collider>();
            if (cols.Length == 0)
            {
                MeshFilter[] mfs = obj.GetComponentsInChildren<MeshFilter>();
                if (mfs.Length > 0)
                {
                    foreach (var mf in mfs)
                    {
                        if (mf.GetComponent<Collider>() == null)
                        {
                            MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                            mc.convex = true;
                        }
                    }
                }
                else
                {
                    obj.AddComponent<BoxCollider>();
                }
            }
        }

        private static void SetupLighting(GameObject parent, float wallHeight)
        {
            GameObject lightObj = new GameObject("Room_Ceiling_WarmLight");
            lightObj.transform.parent = parent.transform;
            lightObj.transform.position = new Vector3(0, wallHeight - 0.3f, 0);

            Light ptLight = lightObj.AddComponent<Light>();
            ptLight.type = LightType.Point;
            ptLight.color = new Color(1f, 0.92f, 0.78f); // Ánh sáng vàng ấm
            ptLight.intensity = 18f;
            ptLight.range = 14f;
            ptLight.shadows = LightShadows.Soft;
        }

        [MenuItem("Tools/📦 Link 3D Models to 30 ItemData Assets")]
        public static void LinkItemModels()
        {
            string itemsFolder = "Assets/ScriptableObjects/Items";
            string modelsFolder = "Assets/Models/FBX format";

            var modelMap = new System.Collections.Generic.Dictionary<string, string>
            {
                { "item_old_book", "books.fbx" },
                { "item_magazine_bundle", "books.fbx" },
                { "item_cardboard_box", "cardboardBoxClosed.fbx" },
                { "item_old_mug", "mug.fbx" },
                { "item_frying_pan", "pan.fbx" },
                { "item_wall_clock", "wallClock.fbx" },
                { "item_antique_clock", "wallClock.fbx" },
                { "item_keyboard", "computerKeyboard.fbx" },
                { "item_mouse", "computerMouse.fbx" },
                { "item_broken_laptop", "laptop.fbx" },
                { "item_rubber_duck", "bear.fbx" },
                { "item_vintage_radio", "radio.fbx" }
            };

            int linkedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { itemsFolder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null && modelMap.TryGetValue(item.id, out string fbxName))
                {
                    string fbxPath = $"{modelsFolder}/{fbxName}";
                    GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (modelPrefab != null)
                    {
                        item.prefab = modelPrefab;
                        EditorUtility.SetDirty(item);
                        linkedCount++;
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Item Models Linked", $"Successfully linked 3D Models to {linkedCount} ItemData assets!", "Great!");
            Debug.Log($"<color=green>[RoomSceneBuilder] Successfully linked {linkedCount} 3D Item Models!</color>");
        }
    }
}

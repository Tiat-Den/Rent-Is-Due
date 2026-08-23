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
        [MenuItem("Tools/🏠 Build Tiny Room (8x6m - Chuẩn MVP)")]
        public static void BuildTinyRoom()
        {
            BuildRoomInternal("Tiny Room (8x6m)", 8f, 6f, 2.6f, isApartment: false);
        }

        [MenuItem("Tools/🏢 Build Apartment (12x9m - Chuẩn Early Game)")]
        public static void BuildApartment()
        {
            BuildRoomInternal("Apartment (12x9m)", 12f, 9f, 2.8f, isApartment: true);
        }

        private static void BuildRoomInternal(string roomTitle, float roomWidth, float roomDepth, float wallHeight, bool isApartment)
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
            Undo.RegisterCreatedObjectUndo(roomRoot, $"Build {roomTitle}");

            // 1. Tạo Vỏ Phòng (Sàn gỗ, 4 Tường có màu, Trần)
            CreateRoomShell(roomRoot, roomWidth, roomDepth, wallHeight);

            // Hệ số Scale chuẩn hóa cho Asset Kenney để khớp đúng kích thước trong WORLD_SCALE_AND_OBJECT_DIMENSIONS.md
            float modelScale = 0.72f;

            if (!isApartment)
            {
                // ==================== TINY ROOM (8m x 6m) ====================
                // Tọa độ phòng: X từ -4.0m đến +4.0m (Rộng 8m), Z từ -3.0m đến +3.0m (Dài 6m)
                
                // 🛏️ Góc Ngủ (Góc Tây Bắc - Sát góc tường X: -3.0, Z: +2.0)
                GameObject bed = SpawnModel(modelsFolder, "bedSingle.fbx", new Vector3(-2.8f, 0, 1.8f), Quaternion.Euler(0, 90, 0), roomRoot, modelScale);
                SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-2.8f, 0, 0.4f), Quaternion.Euler(0, 90, 0), roomRoot, modelScale);
                SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-3.5f, 0, 0.4f), Quaternion.identity, roomRoot, modelScale);

                // 💻 Bàn Làm Việc & Nâng Cấp PC (Góc Đông Bắc - Sát tường Bắc X: +1.8, Z: +2.4)
                GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(1.8f, 0, 2.4f), Quaternion.Euler(0, 180, 0), roomRoot, modelScale);
                SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(1.8f, 0, 1.5f), Quaternion.identity, roomRoot, modelScale);
                SpawnModel(modelsFolder, "computerScreen.fbx", new Vector3(1.4f, 0.55f, 2.4f), Quaternion.Euler(0, 180, 0), roomRoot, modelScale);
                
                // Laptop mở cửa hàng Nâng Cấp
                GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(2.1f, 0.55f, 2.4f), Quaternion.Euler(0, 160, 0), roomRoot, modelScale);
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

                // 🗑️ Góc Rác & Ve Chai (Góc trên cùng mép phải X: +3.4, Z: +2.4)
                GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(3.4f, 0, 2.4f), Quaternion.Euler(0, -35, 0), roomRoot, modelScale);
                if (trash != null)
                {
                    EnsureCollider(trash);
                    SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                    trashSearch.containerName = "Trash Pile";
                    trashSearch.searchDuration = 1.5f;
                    trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
                }

                // 🍳 Kệ Bếp (Mép tường Đông X: +3.5, Z: +0.2)
                GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(3.5f, 0, 0.2f), Quaternion.Euler(0, -90, 0), roomRoot, modelScale);
                if (kitchen != null)
                {
                    EnsureCollider(kitchen);
                    SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                    kitchenSearch.containerName = "Kitchen Cabinet";
                    kitchenSearch.searchDuration = 2.0f;
                    kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
                }

                // 🚪 Tủ Quần Áo (Mép tường Tây X: -3.5, Z: -1.2)
                GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-3.5f, 0, -1.2f), Quaternion.Euler(0, 90, 0), roomRoot, modelScale);
                if (wardrobe != null)
                {
                    EnsureCollider(wardrobe);
                    SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                    wardrobeSearch.containerName = "Wardrobe";
                    wardrobeSearch.searchDuration = 3.0f;
                    wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
                }

                // 🔒 Két Sắt Bí Mật (Góc Tây Nam X: -3.4, Z: -2.4)
                GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-3.4f, 0, -2.4f), Quaternion.Euler(0, 45, 0), roomRoot, modelScale);
                if (safe != null)
                {
                    EnsureCollider(safe);
                    SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                    safeSearch.containerName = "Secret Safe";
                    safeSearch.searchDuration = 4.0f;
                    safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
                }

                // 👤 Quầy Dealer Ve Chai (Góc Đông Nam gần cửa X: +3.0, Z: -2.2)
                GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(3.0f, 0, -2.2f), Quaternion.Euler(0, -90, 0), roomRoot, modelScale);
                if (dealerDesk != null)
                {
                    EnsureCollider(dealerDesk);
                    dealerDesk.AddComponent<DealerInteractable>();
                }
            }
            else
            {
                // ==================== APARTMENT (12m x 9m) ====================
                // Tọa độ phòng: X: [-6.0, +6.0], Z: [-4.5, +4.5], Height: 2.8m
                
                // Khu Phòng Ngủ (Tây Bắc: -4.5, +3.0)
                SpawnModel(modelsFolder, "bedDouble.fbx", new Vector3(-4.5f, 0, 3.0f), Quaternion.Euler(0, 90, 0), roomRoot, 0.85f);
                SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-4.5f, 0, 1.2f), Quaternion.Euler(0, 90, 0), roomRoot, 0.85f);
                SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-5.4f, 0, 1.2f), Quaternion.identity, roomRoot, 0.85f);

                // Khu Làm Việc & Nâng Cấp (Chính Bắc: +0.5, +3.8)
                GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(0.5f, 0, 3.8f), Quaternion.Euler(0, 180, 0), roomRoot, 0.85f);
                SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(0.5f, 0, 2.6f), Quaternion.identity, roomRoot, 0.85f);
                SpawnModel(modelsFolder, "computerScreen.fbx", new Vector3(0.1f, 0.65f, 3.8f), Quaternion.Euler(0, 180, 0), roomRoot, 0.85f);
                
                GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(0.9f, 0.65f, 3.8f), Quaternion.Euler(0, 160, 0), roomRoot, 0.85f);
                if (laptop != null)
                {
                    EnsureCollider(laptop);
                    laptop.AddComponent<UpgradeInteractable>();
                }

                if (desk != null)
                {
                    EnsureCollider(desk);
                    SearchableObject deskSearch = desk.AddComponent<SearchableObject>();
                    deskSearch.containerName = "Study Desk";
                    deskSearch.searchDuration = 2.5f;
                    deskSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/DeskLootTable.asset");
                }

                // Khu Bếp (Đông Bắc: +5.0, +3.2)
                GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(5.2f, 0, 3.2f), Quaternion.Euler(0, -90, 0), roomRoot, 0.85f);
                SpawnModel(modelsFolder, "kitchenSink.fbx", new Vector3(5.2f, 0, 1.6f), Quaternion.Euler(0, -90, 0), roomRoot, 0.85f);
                if (kitchen != null)
                {
                    EnsureCollider(kitchen);
                    SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                    kitchenSearch.containerName = "Kitchen Cabinet";
                    kitchenSearch.searchDuration = 2.0f;
                    kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
                }

                // Góc Rác (Phía Đông: +5.2, -0.5)
                GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(5.2f, 0, -0.5f), Quaternion.Euler(0, -35, 0), roomRoot, 0.85f);
                if (trash != null)
                {
                    EnsureCollider(trash);
                    SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                    trashSearch.containerName = "Trash Pile";
                    trashSearch.searchDuration = 1.5f;
                    trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
                }

                // Tủ Quần Áo & Kệ Sách (Tây Nam: -5.2, -1.5)
                GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-5.2f, 0, -1.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.85f);
                SpawnModel(modelsFolder, "bookcaseOpen.fbx", new Vector3(-5.2f, 0, -3.0f), Quaternion.Euler(0, 90, 0), roomRoot, 0.85f);
                if (wardrobe != null)
                {
                    EnsureCollider(wardrobe);
                    SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                    wardrobeSearch.containerName = "Wardrobe";
                    wardrobeSearch.searchDuration = 3.0f;
                    wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
                }

                // Két Sắt Bí Mật (Góc Tây Nam: -4.5, -4.0)
                GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-4.5f, 0, -4.0f), Quaternion.Euler(0, 45, 0), roomRoot, 0.85f);
                if (safe != null)
                {
                    EnsureCollider(safe);
                    SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                    safeSearch.containerName = "Secret Safe";
                    safeSearch.searchDuration = 4.0f;
                    safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
                }

                // Quầy Dealer Ve Chai (Đông Nam: +4.8, -3.8)
                GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(4.8f, 0, -3.8f), Quaternion.Euler(0, -45, 0), roomRoot, 0.85f);
                if (dealerDesk != null)
                {
                    EnsureCollider(dealerDesk);
                    dealerDesk.AddComponent<DealerInteractable>();
                }
            }

            // Thiết lập ánh sáng phòng ấm cúng
            SetupLighting(roomRoot, wallHeight);

            // Đặt Player vào tâm phòng
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1.0f, 0);
            }

            EditorUtility.DisplayDialog($"{roomTitle} Built!", $"{roomTitle} environment built according to WORLD_SCALE_AND_OBJECT_DIMENSIONS standards!", "Great!");
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
            floor.transform.parent = parent.transform;
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);

            // 4 Bức tường chuẩn độ dày 0.2m và chiều cao wallHeight
            CreateWall("Wall_North", new Vector3(0, halfH, halfD), new Vector3(width, height, 0.2f), parent, new Color(0.9f, 0.88f, 0.84f));
            CreateWall("Wall_South", new Vector3(0, halfH, -halfD), new Vector3(width, height, 0.2f), parent, new Color(0.9f, 0.88f, 0.84f));
            CreateWall("Wall_West", new Vector3(-halfW, halfH, 0), new Vector3(0.2f, height, depth), parent, new Color(0.85f, 0.83f, 0.80f));
            CreateWall("Wall_East", new Vector3(halfW, halfH, 0), new Vector3(0.2f, height, depth), parent, new Color(0.85f, 0.83f, 0.80f));

            // Trần nhà (Ceiling)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Room_Ceiling";
            ceiling.transform.parent = parent.transform;
            ceiling.transform.position = new Vector3(0, height, 0);
            ceiling.transform.rotation = Quaternion.Euler(180, 0, 0);
            ceiling.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);
        }

        private static void CreateWall(string name, Vector3 pos, Vector3 size, GameObject parent, Color wallColor)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent.transform;
            wall.transform.position = pos;
            wall.transform.localScale = size;

            // Gán màu sơn tường trang nhã
            Renderer rend = wall.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = wallColor
                };
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
            instance.transform.parent = parent.transform;
            instance.transform.position = pos;
            instance.transform.rotation = rot;
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

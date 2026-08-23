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

            // 1. Tạo Vỏ Phòng (Sàn, 4 Tường, Trần) theo đúng kích thước meter chuẩn
            CreateRoomShell(roomRoot, roomWidth, roomDepth, wallHeight);

            if (!isApartment)
            {
                // ==================== TINY ROOM (8m x 6m) ====================
                // X: [-4.0, +4.0], Z: [-3.0, +3.0], Height: 2.6m
                
                // Giường ngủ (Góc trên-trái)
                GameObject bed = SpawnModel(modelsFolder, "bedSingle.fbx", new Vector3(-2.8f, 0, 1.8f), Quaternion.Euler(0, 90, 0), roomRoot);
                SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-2.8f, 0, 0.5f), Quaternion.Euler(0, 90, 0), roomRoot);
                SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-3.4f, 0, 0.5f), Quaternion.identity, roomRoot);

                // Bàn làm việc & Laptop Nâng Cấp (Góc trên-phải)
                GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(1.5f, 0, 2.3f), Quaternion.Euler(0, 180, 0), roomRoot);
                SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(1.5f, 0, 1.4f), Quaternion.identity, roomRoot);
                SpawnModel(modelsFolder, "computerScreen.fbx", new Vector3(1.1f, 0.75f, 2.3f), Quaternion.Euler(0, 180, 0), roomRoot);
                
                GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(1.9f, 0.75f, 2.3f), Quaternion.Euler(0, 160, 0), roomRoot);
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

                // Góc Rác (Góc trên-cùng mép phải)
                GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(3.2f, 0, 2.2f), Quaternion.Euler(0, -30, 0), roomRoot);
                if (trash != null)
                {
                    EnsureCollider(trash);
                    SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                    trashSearch.containerName = "Trash Pile";
                    trashSearch.searchDuration = 1.5f;
                    trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
                }

                // Kệ bếp (Mép tường phải)
                GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(3.3f, 0, 0.2f), Quaternion.Euler(0, -90, 0), roomRoot);
                if (kitchen != null)
                {
                    EnsureCollider(kitchen);
                    SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                    kitchenSearch.containerName = "Kitchen Cabinet";
                    kitchenSearch.searchDuration = 2.0f;
                    kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
                }

                // Tủ quần áo & Giá sách (Góc dưới-trái)
                GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-3.3f, 0, -1.6f), Quaternion.Euler(0, 90, 0), roomRoot);
                if (wardrobe != null)
                {
                    EnsureCollider(wardrobe);
                    SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                    wardrobeSearch.containerName = "Wardrobe";
                    wardrobeSearch.searchDuration = 3.0f;
                    wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
                }

                // Két sắt bí mật (Góc dưới-trái giấu kín)
                GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-3.3f, 0, -2.4f), Quaternion.Euler(0, 45, 0), roomRoot);
                if (safe != null)
                {
                    EnsureCollider(safe);
                    SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                    safeSearch.containerName = "Secret Safe";
                    safeSearch.searchDuration = 4.0f;
                    safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
                }

                // Quầy giao dịch Dealer Ve Chai (Góc dưới-phải gần cửa)
                GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(3.0f, 0, -2.0f), Quaternion.Euler(0, -90, 0), roomRoot);
                if (dealerDesk != null)
                {
                    EnsureCollider(dealerDesk);
                    dealerDesk.AddComponent<DealerInteractable>();
                }
            }
            else
            {
                // ==================== APARTMENT (12m x 9m) ====================
                // X: [-6.0, +6.0], Z: [-4.5, +4.5], Height: 2.8m
                
                // Khu Phòng Ngủ (Tây Bắc: -4.0, +2.5)
                SpawnModel(modelsFolder, "bedDouble.fbx", new Vector3(-4.2f, 0, 2.8f), Quaternion.Euler(0, 90, 0), roomRoot);
                SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-4.2f, 0, 1.2f), Quaternion.Euler(0, 90, 0), roomRoot);
                SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-5.2f, 0, 1.2f), Quaternion.identity, roomRoot);

                // Khu Làm Việc & Nâng Cấp (Chính Bắc)
                GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(0.5f, 0, 3.8f), Quaternion.Euler(0, 180, 0), roomRoot);
                SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(0.5f, 0, 2.7f), Quaternion.identity, roomRoot);
                SpawnModel(modelsFolder, "computerScreen.fbx", new Vector3(0.1f, 0.75f, 3.8f), Quaternion.Euler(0, 180, 0), roomRoot);
                
                GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(0.9f, 0.75f, 3.8f), Quaternion.Euler(0, 160, 0), roomRoot);
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

                // Khu Bếp (Đông Bắc: +4.5, +2.5)
                GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(5.0f, 0, 3.5f), Quaternion.Euler(0, -90, 0), roomRoot);
                SpawnModel(modelsFolder, "kitchenSink.fbx", new Vector3(5.0f, 0, 2.0f), Quaternion.Euler(0, -90, 0), roomRoot);
                if (kitchen != null)
                {
                    EnsureCollider(kitchen);
                    SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                    kitchenSearch.containerName = "Kitchen Cabinet";
                    kitchenSearch.searchDuration = 2.0f;
                    kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
                }

                // Góc Rác (Phía Đông)
                GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(5.0f, 0, 0.5f), Quaternion.Euler(0, -35, 0), roomRoot);
                if (trash != null)
                {
                    EnsureCollider(trash);
                    SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                    trashSearch.containerName = "Trash Pile";
                    trashSearch.searchDuration = 1.5f;
                    trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
                }

                // Tủ Quần Áo & Kệ Sách (Tây Nam)
                GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-5.0f, 0, -1.5f), Quaternion.Euler(0, 90, 0), roomRoot);
                SpawnModel(modelsFolder, "bookcaseOpen.fbx", new Vector3(-5.0f, 0, -3.0f), Quaternion.Euler(0, 90, 0), roomRoot);
                if (wardrobe != null)
                {
                    EnsureCollider(wardrobe);
                    SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                    wardrobeSearch.containerName = "Wardrobe";
                    wardrobeSearch.searchDuration = 3.0f;
                    wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
                }

                // Két Sắt Bí Mật
                GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-4.2f, 0, -3.8f), Quaternion.Euler(0, 45, 0), roomRoot);
                if (safe != null)
                {
                    EnsureCollider(safe);
                    SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                    safeSearch.containerName = "Secret Safe";
                    safeSearch.searchDuration = 4.0f;
                    safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
                }

                // Quầy Dealer Ve Chai (Đông Nam)
                GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(4.5f, 0, -3.5f), Quaternion.Euler(0, -45, 0), roomRoot);
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
            CreateWall("Wall_North", new Vector3(0, halfH, halfD), new Vector3(width, height, 0.2f), parent);
            CreateWall("Wall_South", new Vector3(0, halfH, -halfD), new Vector3(width, height, 0.2f), parent);
            CreateWall("Wall_West", new Vector3(-halfW, halfH, 0), new Vector3(0.2f, height, depth), parent);
            CreateWall("Wall_East", new Vector3(halfW, halfH, 0), new Vector3(0.2f, height, depth), parent);

            // Trần nhà (Ceiling)
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Room_Ceiling";
            ceiling.transform.parent = parent.transform;
            ceiling.transform.position = new Vector3(0, height, 0);
            ceiling.transform.rotation = Quaternion.Euler(180, 0, 0);
            ceiling.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);
        }

        private static void CreateWall(string name, Vector3 pos, Vector3 size, GameObject parent)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent.transform;
            wall.transform.position = pos;
            wall.transform.localScale = size;
        }

        private static GameObject SpawnModel(string folder, string fileName, Vector3 pos, Quaternion rot, GameObject parent)
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

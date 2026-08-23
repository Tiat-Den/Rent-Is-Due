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
        [MenuItem("Tools/🏠 Build 3D Furniture Room Scene")]
        public static void BuildRoom()
        {
            string modelsFolder = "Assets/Models/FBX format";

            // Tìm hoặc tạo GameObject gốc chứa toàn bộ căn phòng
            GameObject roomRoot = GameObject.Find("TinyRoom_Environment");
            if (roomRoot != null)
            {
                if (EditorUtility.DisplayDialog("Rebuild Room", "TinyRoom_Environment already exists. Do you want to replace it?", "Yes, Rebuild", "Cancel"))
                {
                    Undo.DestroyObjectImmediate(roomRoot);
                }
                else
                {
                    return;
                }
            }

            roomRoot = new GameObject("TinyRoom_Environment");
            Undo.RegisterCreatedObjectUndo(roomRoot, "Build 3D Room");

            // 1. Tạo Sàn nhà & Trần nhà (Floor & Ceiling)
            CreateRoomShell(roomRoot);

            // 2. Kê Giường Ngủ (Bed Area)
            GameObject bed = SpawnModel(modelsFolder, "bedSingle.fbx", new Vector3(-3.2f, 0, 2.2f), Quaternion.Euler(0, 90, 0), roomRoot);
            SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-3.2f, 0, 3.5f), Quaternion.Euler(0, 90, 0), roomRoot);
            SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-4.0f, 0, 3.5f), Quaternion.identity, roomRoot);

            // 3. Kê Bàn Làm Việc (Study Desk & Laptop Shop)
            GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(-1.0f, 0, 3.4f), Quaternion.Euler(0, 180, 0), roomRoot);
            SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(-1.0f, 0, 2.4f), Quaternion.identity, roomRoot);
            SpawnModel(modelsFolder, "computerScreen.fbx", new Vector3(-1.4f, 0.75f, 3.4f), Quaternion.Euler(0, 180, 0), roomRoot);
            
            // Laptop mở cửa hàng Nâng Cấp
            GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(-0.6f, 0.75f, 3.4f), Quaternion.Euler(0, 160, 0), roomRoot);
            if (laptop != null)
            {
                EnsureCollider(laptop);
                UpgradeInteractable upgrade = laptop.AddComponent<UpgradeInteractable>();
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

            // 4. Góc Thùng Rác (Trash Bin)
            GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(3.8f, 0, 3.3f), Quaternion.Euler(0, -25, 0), roomRoot);
            if (trash != null)
            {
                EnsureCollider(trash);
                SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                trashSearch.containerName = "Trash Pile";
                trashSearch.searchDuration = 1.5f;
                trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
            }

            // 5. Khu Tủ Bếp (Kitchen Area)
            GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(3.5f, 0, 0f), Quaternion.Euler(0, -90, 0), roomRoot);
            SpawnModel(modelsFolder, "kitchenSink.fbx", new Vector3(3.5f, 0, 1.2f), Quaternion.Euler(0, -90, 0), roomRoot);
            if (kitchen != null)
            {
                EnsureCollider(kitchen);
                SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                kitchenSearch.containerName = "Kitchen Cabinet";
                kitchenSearch.searchDuration = 2.0f;
                kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
            }

            // 6. Tủ Quần Áo & Giá Sách (Wardrobe & Bookshelf)
            GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-4.0f, 0, 0f), Quaternion.Euler(0, 90, 0), roomRoot);
            SpawnModel(modelsFolder, "bookcaseOpen.fbx", new Vector3(-4.0f, 0, -1.2f), Quaternion.Euler(0, 90, 0), roomRoot);
            if (wardrobe != null)
            {
                EnsureCollider(wardrobe);
                SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                wardrobeSearch.containerName = "Wardrobe";
                wardrobeSearch.searchDuration = 3.0f;
                wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
            }

            // 7. Két Sắt Bí Mật (Secret Safe giấu kín)
            GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-3.8f, 0, -2.8f), Quaternion.Euler(0, 35, 0), roomRoot);
            if (safe != null)
            {
                EnsureCollider(safe);
                SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                safeSearch.containerName = "Secret Safe";
                safeSearch.searchDuration = 4.0f;
                safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
            }

            // 8. Bàn Giao Dịch của Ông Dealer ve chai (Near Door)
            GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(3.2f, 0, -3.2f), Quaternion.Euler(0, -45, 0), roomRoot);
            if (dealerDesk != null)
            {
                EnsureCollider(dealerDesk);
                DealerInteractable dealer = dealerDesk.AddComponent<DealerInteractable>();
            }

            // 9. Hệ thống Ánh Sáng Phòng Ấm Cúng (Warm Lighting)
            SetupLighting(roomRoot);

            // 10. Định vị lại Player
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1.0f, -1.0f);
            }

            EditorUtility.DisplayDialog("Room Built Successfully", "3D Tiny Room environment has been built with real furniture models and interactive loot spots!", "Awesome!");
            Debug.Log("<color=green>[RoomSceneBuilder] Successfully built 3D Tiny Room!</color>");
        }

        private static void CreateRoomShell(GameObject parent)
        {
            // Sàn nhà (Floor 10x8)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Room_Floor";
            floor.transform.parent = parent.transform;
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(1.0f, 1f, 0.8f);

            // 4 Bức tường bao quanh
            CreateWall("Wall_North", new Vector3(0, 2f, 4f), new Vector3(10f, 4f, 0.3f), parent);
            CreateWall("Wall_South", new Vector3(0, 2f, -4f), new Vector3(10f, 4f, 0.3f), parent);
            CreateWall("Wall_West", new Vector3(-5f, 2f, 0), new Vector3(0.3f, 4f, 8f), parent);
            CreateWall("Wall_East", new Vector3(5f, 2f, 0), new Vector3(0.3f, 4f, 8f), parent);

            // Trần nhà
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Room_Ceiling";
            ceiling.transform.parent = parent.transform;
            ceiling.transform.position = new Vector3(0, 4f, 0);
            ceiling.transform.rotation = Quaternion.Euler(180, 0, 0);
            ceiling.transform.localScale = new Vector3(1.0f, 1f, 0.8f);
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

        private static void SetupLighting(GameObject parent)
        {
            GameObject lightObj = new GameObject("Room_Ceiling_WarmLight");
            lightObj.transform.parent = parent.transform;
            lightObj.transform.position = new Vector3(0, 3.2f, 0);

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

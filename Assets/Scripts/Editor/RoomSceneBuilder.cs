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
    public class RoomSceneBuilder : EditorWindow
    {
        [MenuItem("Tools/🏠 Custom Room Builder Window (Tùy Chỉnh Kích Thước)")]
        public static void OpenWindow()
        {
            RoomSceneBuilder window = GetWindow<RoomSceneBuilder>("Room Builder");
            window.minSize = new Vector2(380, 320);
            window.Show();
        }

        [MenuItem("Tools/🏠 Build Giant Room (25m x 20m - Siêu Rộng Rãi)")]
        public static void BuildGiantRoom()
        {
            BuildRoomInternal("Giant Room (25m x 20m)", 25f, 20f, 4.5f);
        }

        [MenuItem("Tools/🏠 Build Spacious Room (16m x 14m)")]
        public static void BuildSpaciousRoom()
        {
            BuildRoomInternal("Spacious Room (16m x 14m)", 16f, 14f, 4.0f);
        }

        private float customWidth = 25f;
        private float customDepth = 20f;
        private float customHeight = 4.5f;

        private void OnGUI()
        {
            GUILayout.Label("🏠 <b>RENT IS DUE — 3D ROOM BUILDER</b>", EditorStyles.boldLabel);
            GUILayout.Space(10);

            customWidth = EditorGUILayout.Slider("Chiều Rộng (Width - mét):", customWidth, 10f, 50f);
            customDepth = EditorGUILayout.Slider("Chiều Dài (Depth - mét):", customDepth, 10f, 50f);
            customHeight = EditorGUILayout.Slider("Chiều Cao Trần (Height):", customHeight, 3f, 8f);

            GUILayout.Space(15);
            GUILayout.Label($"Diện tích sàn: <b>{(int)(customWidth * customDepth)} m²</b>", EditorStyles.helpBox);

            GUILayout.Space(15);
            if (GUILayout.Button($"🚀 XÂY PHÒNG NGAY ({customWidth:F0}m x {customDepth:F0}m)", GUILayout.Height(45)))
            {
                BuildRoomInternal($"Custom Room ({customWidth:F0}m x {customDepth:F0}m)", customWidth, customDepth, customHeight);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("📦 Link 3D Models to 30 ItemData Assets", GUILayout.Height(30)))
            {
                LinkItemModels();
            }
        }

        public static void BuildRoomInternal(string roomTitle, float roomWidth, float roomDepth, float wallHeight)
        {
            string modelsFolder = "Assets/Models/FBX format";

            GameObject roomRoot = GameObject.Find("TinyRoom_Environment");
            if (roomRoot != null)
            {
                Undo.DestroyObjectImmediate(roomRoot);
            }

            roomRoot = new GameObject("TinyRoom_Environment");
            roomRoot.transform.position = Vector3.zero;
            roomRoot.transform.rotation = Quaternion.identity;
            roomRoot.transform.localScale = Vector3.one;
            Undo.RegisterCreatedObjectUndo(roomRoot, $"Build {roomTitle}");

            // 1. Tạo Vỏ Phòng dày dặn chống rách bóng (Floor Cube 0.3m, 4 Tường, Trần)
            CreateRoomShell(roomRoot, roomWidth, roomDepth, wallHeight);

            float halfW = roomWidth / 2f;
            float halfD = roomDepth / 2f;

            // 2. 🛏️ GÓC GIƯỜNG NGỦ (Góc Tây Bắc - Scale 0.60x)
            GameObject bed = SpawnModel(modelsFolder, "bedSingle.fbx", new Vector3(-halfW + 2.0f, 0, halfD - 2.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.60f);
            SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-halfW + 2.0f, 0, halfD - 4.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.60f);
            SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-halfW + 1.0f, 0, halfD - 4.5f), Quaternion.identity, roomRoot, 0.60f);

            // 3. 💻 BÀN LÀM VIỆC & NÂNG CẤP (Phía Đông Bắc - Scale 0.58x)
            GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(3.0f, 0, halfD - 1.5f), Quaternion.Euler(0, 180, 0), roomRoot, 0.58f);
            
            // Ghế xoay đặt đúng phía trước bàn làm việc (Scale 0.58x)
            SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(3.0f, 0, halfD - 2.4f), Quaternion.identity, roomRoot, 0.58f);
            
            // Laptop nhỏ gọn đặt ngay ngắn trên mặt bàn (Scale 0.28x)
            GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(3.0f, 0.52f, halfD - 1.5f), Quaternion.Euler(0, 180, 0), roomRoot, 0.28f);
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

            // 4. 🗑️ GÓC THÙNG RÁC (Góc Đông Bắc sát tường - Scale 0.60x)
            GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(halfW - 1.8f, 0, halfD - 1.8f), Quaternion.Euler(0, -35, 0), roomRoot, 0.60f);
            if (trash != null)
            {
                EnsureCollider(trash);
                SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                trashSearch.containerName = "Trash Pile";
                trashSearch.searchDuration = 1.5f;
                trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
            }

            // 5. 🍳 KHU BẾP (Mép tường phía Đông - Scale 0.60x)
            GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(halfW - 1.2f, 0, 0.5f), Quaternion.Euler(0, -90, 0), roomRoot, 0.60f);
            SpawnModel(modelsFolder, "kitchenSink.fbx", new Vector3(halfW - 1.2f, 0, -1.0f), Quaternion.Euler(0, -90, 0), roomRoot, 0.60f);
            if (kitchen != null)
            {
                EnsureCollider(kitchen);
                SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                kitchenSearch.containerName = "Kitchen Cabinet";
                kitchenSearch.searchDuration = 2.0f;
                kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
            }

            // 6. 🚪 TỦ QUẦN ÁO & GIÁ SÁCH (Mép tường phía Tây - Scale 0.60x)
            GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-halfW + 1.2f, 0, -0.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.60f);
            SpawnModel(modelsFolder, "bookcaseOpen.fbx", new Vector3(-halfW + 1.2f, 0, -2.0f), Quaternion.Euler(0, 90, 0), roomRoot, 0.60f);
            if (wardrobe != null)
            {
                EnsureCollider(wardrobe);
                SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                wardrobeSearch.containerName = "Wardrobe";
                wardrobeSearch.searchDuration = 3.0f;
                wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
            }

            // 7. 🔒 KÉT SẮT BÍ MẬT (Góc Tây Nam - Scale 0.60x)
            GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-halfW + 2.0f, 0, -halfD + 2.0f), Quaternion.Euler(0, 45, 0), roomRoot, 0.60f);
            if (safe != null)
            {
                EnsureCollider(safe);
                SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                safeSearch.containerName = "Secret Safe";
                safeSearch.searchDuration = 4.0f;
                safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
            }

            // 8. 👤 QUẦY DEALER VE CHAI (Góc Đông Nam - Scale 0.60x)
            GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(halfW - 2.5f, 0, -halfD + 2.5f), Quaternion.Euler(0, -45, 0), roomRoot, 0.60f);
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

            Debug.Log($"<color=green>[RoomSceneBuilder] Successfully built {roomTitle} ({roomWidth}m x {roomDepth}m)!</color>");
        }

        private static void CreateRoomShell(GameObject parent, float width, float depth, float height)
        {
            float halfW = width / 2f;
            float halfD = depth / 2f;
            float halfH = height / 2f;

            // Sàn nhà dày (Floor Cube 0.3m chống rách bóng)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Room_Floor";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(0, -0.15f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
            ApplyMaterial(floor, new Color(0.32f, 0.25f, 0.20f)); // Sàn gỗ nâu ấm

            // 4 Bức tường bao quanh
            CreateWall("Wall_North", new Vector3(0, halfH, halfD), new Vector3(width, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            CreateWall("Wall_South", new Vector3(0, halfH, -halfD), new Vector3(width, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            CreateWall("Wall_West", new Vector3(-halfW, halfH, 0), new Vector3(0.3f, height, depth), parent, new Color(0.82f, 0.80f, 0.76f));
            CreateWall("Wall_East", new Vector3(halfW, halfH, 0), new Vector3(0.3f, height, depth), parent, new Color(0.82f, 0.80f, 0.76f));

            // Trần nhà
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Room_Ceiling";
            ceiling.transform.SetParent(parent.transform, false);
            ceiling.transform.localPosition = new Vector3(0, height + 0.15f, 0);
            ceiling.transform.localScale = new Vector3(width, 0.3f, depth);
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

using System.IO;
using UnityEngine;
using UnityEditor;
using RentIsDue.Core;
using RentIsDue.Economy;
using RentIsDue.Inventory;
using RentIsDue.Loot;
using RentIsDue.Shop;
using RentIsDue.Environment;

namespace RentIsDue.Editor
{
    public class RoomSceneBuilder : EditorWindow
    {
        private const string SAVED_ROOM_PREFAB_PATH = "Assets/Prefabs/Environments/SavedGiantRoom.prefab";

        [MenuItem("Tools/🏠 Build Giant Room (25m x 20m - Siêu Rộng Rãi)")]
        public static void BuildGiantRoom()
        {
            BuildRoomInternal("Giant Room (25m x 20m)", 25f, 20f, 4.5f);
        }

        [MenuItem("Tools/💾 Save Current Room Layout as Custom Template")]
        public static void SaveCurrentRoomLayout()
        {
            GameObject roomRoot = GameObject.Find("TinyRoom_Environment");
            if (roomRoot == null)
            {
                EditorUtility.DisplayDialog("Room Not Found", "Không tìm thấy GameObject 'TinyRoom_Environment' trong Hierarchy để lưu! Hãy chắc chắn căn phòng đang mở trong Scene.", "OK");
                return;
            }

            string folder = "Assets/Prefabs/Environments";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(roomRoot, SAVED_ROOM_PREFAB_PATH, InteractionMode.UserAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Room Saved! 💾", "Đã lưu thành công toàn bộ cách sắp xếp căn phòng của bạn vào Template Prefab (SavedGiantRoom.prefab)!\n\nTừ bây giờ bạn có thể nạp lại bất cứ lúc nào qua menu 'Restore Saved Custom Room Template'.", "Tuyệt vời!");
            Debug.Log($"<color=green>[RoomSceneBuilder] Successfully saved room layout to {SAVED_ROOM_PREFAB_PATH}!</color>");
        }

        [MenuItem("Tools/🏠 Restore Saved Custom Room Template (Nạp Phòng Đã Lưu)")]
        public static void RestoreSavedRoom()
        {
            if (!File.Exists(SAVED_ROOM_PREFAB_PATH))
            {
                EditorUtility.DisplayDialog("No Saved Room", "Chưa có file phòng mẫu nào được lưu! Hãy bấm 'Save Current Room Layout as Custom Template' trước.", "OK");
                return;
            }

            GameObject existingRoom = GameObject.Find("TinyRoom_Environment");
            if (existingRoom != null)
            {
                Undo.DestroyObjectImmediate(existingRoom);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(SAVED_ROOM_PREFAB_PATH);
            if (prefab != null)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = "TinyRoom_Environment";
                instance.transform.position = Vector3.zero;
                instance.transform.rotation = Quaternion.identity;
                Undo.RegisterCreatedObjectUndo(instance, "Restore Saved Giant Room");

                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    player.transform.position = new Vector3(0, 1.0f, 0);
                }

                Debug.Log("<color=green>[RoomSceneBuilder] Restored saved custom room template!</color>");
            }
        }

        [MenuItem("Tools/🏠 Build Spacious Room (16m x 14m)")]
        public static void BuildSpaciousRoom()
        {
            BuildRoomInternal("Spacious Room (16m x 14m)", 16f, 14f, 4.0f);
        }

        [MenuItem("Tools/🏠 Custom Room Builder Window (Tùy Chỉnh Kích Thước)")]
        public static void OpenWindow()
        {
            RoomSceneBuilder window = GetWindow<RoomSceneBuilder>("Room Builder");
            window.minSize = new Vector2(380, 380);
            window.Show();
        }

        private float customWidth = 25f;
        private float customDepth = 20f;
        private float customHeight = 4.5f;

        private void OnGUI()
        {
            GUILayout.Label("🏠 <b>RENT IS DUE — 3D ROOM BUILDER</b>", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("💾 LƯU BỐ CỤC PHÒNG HIỆN TẠI VÀO TEMPLATE", GUILayout.Height(32)))
            {
                SaveCurrentRoomLayout();
            }

            if (GUILayout.Button("🏠 NẠP LẠI PHÒNG ĐÃ LƯU TÙY CHỈNH", GUILayout.Height(32)))
            {
                RestoreSavedRoom();
            }

            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Bạn có thể tự do kéo thả, dịch chuyển nội thất theo ý thích rồi bấm 'LƯU BỐ CỤC PHÒNG' ở trên để lưu vĩnh viễn.", MessageType.Info);
            GUILayout.Space(10);

            customWidth = EditorGUILayout.Slider("Chiều Rộng (Width - mét):", customWidth, 10f, 50f);
            customDepth = EditorGUILayout.Slider("Chiều Dài (Depth - mét):", customDepth, 10f, 50f);
            customHeight = EditorGUILayout.Slider("Chiều Cao Trần (Height):", customHeight, 3f, 8f);

            GUILayout.Space(10);
            GUILayout.Label($"Diện tích sàn: <b>{(int)(customWidth * customDepth)} m²</b>", EditorStyles.helpBox);

            GUILayout.Space(10);
            if (GUILayout.Button($"🚀 XÂY LẠI TỪ ĐẦU ({customWidth:F0}m x {customDepth:F0}m)", GUILayout.Height(35)))
            {
                BuildRoomInternal($"Custom Room ({customWidth:F0}m x {customDepth:F0}m)", customWidth, customDepth, customHeight);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("📦 Link 3D Models to 30 ItemData Assets", GUILayout.Height(25)))
            {
                LinkItemModels();
            }
        }

        public static void BuildRoomInternal(string roomTitle, float roomWidth, float roomDepth, float wallHeight)
        {
            string modelsFolder = "Assets/Models/FBX format";
            string urbanFolder = "Assets/Models_Item/FBX format";

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

            // 1. Tạo Vỏ Phòng (Sàn, 3 Tường bao, Tường Cửa Sổ phía Nam có khoét khung kính)
            CreateRoomShellWithWindow(roomRoot, roomWidth, roomDepth, wallHeight, urbanFolder);

            float halfW = roomWidth / 2f;
            float halfD = roomDepth / 2f;

            // 2. 🚪 CỬA RA VÀO (Đặt ở mép tường phía Tây - Scale chuẩn 0.75x)
            GameObject door = SpawnModel(urbanFolder, "door-type-a.fbx", new Vector3(-halfW + 0.15f, 0, -halfD + 4.0f), Quaternion.Euler(0, 90, 0), roomRoot, 0.75f);
            if (door != null)
            {
                EnsureCollider(door);
            }

            // 🎛️ CÔNG TẮC ĐÈN TRẦN (Gắn tường cạnh cửa ra vào)
            GameObject switchBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            switchBox.name = "Light_Switch_Wall";
            switchBox.transform.SetParent(roomRoot.transform, false);
            switchBox.transform.localPosition = new Vector3(-halfW + 0.20f, 1.40f, -halfD + 4.8f);
            switchBox.transform.localScale = new Vector3(0.08f, 0.14f, 0.10f);
            ApplyMaterial(switchBox, "Mat_LightSwitch", new Color(0.95f, 0.95f, 0.95f));

            // 3. 💡 ĐÈN TRẦN & BÓNG CHIẾU TỪ TRÊN XUỐNG (Treo giữa phòng)
            GameObject ceilingLamp = SpawnModel(modelsFolder, "lampSquareCeiling.fbx", new Vector3(0, wallHeight - 0.15f, 0), Quaternion.identity, roomRoot, 0.75f);
            
            GameObject ceilingLightObj = new GameObject("Ceiling_Light_Source");
            ceilingLightObj.transform.SetParent(roomRoot.transform, false);
            ceilingLightObj.transform.localPosition = new Vector3(0, wallHeight - 0.6f, 0);

            Light ceilingLight = ceilingLightObj.AddComponent<Light>();
            ceilingLight.type = LightType.Point;
            ceilingLight.color = new Color(1f, 0.94f, 0.82f); // Ánh sáng đèn ấm cúng
            ceilingLight.intensity = 20f;
            ceilingLight.range = 22f;
            ceilingLight.shadows = LightShadows.Soft;
            ceilingLight.enabled = false; // Mặc định tắt ban ngày để đón nắng cửa sổ

            // Gắn tương tác công tắc bật/tắt đèn
            CeilingLightSwitch lightSwitch = switchBox.AddComponent<CeilingLightSwitch>();
            lightSwitch.ceilingLight = ceilingLight;
            lightSwitch.isLightOn = false;

            // 4. 🛏️ GÓC GIƯỜNG NGỦ (Góc Tây Bắc - Scale 0.35x)
            GameObject bed = SpawnModel(modelsFolder, "bedSingle.fbx", new Vector3(-halfW + 2.0f, 0, halfD - 2.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.35f);
            SpawnModel(modelsFolder, "cabinetBed.fbx", new Vector3(-halfW + 2.0f, 0, halfD - 4.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.35f);
            SpawnModel(modelsFolder, "lampRoundFloor.fbx", new Vector3(-halfW + 1.0f, 0, halfD - 4.5f), Quaternion.identity, roomRoot, 0.35f);

            // 5. 💻 BÀN LÀM VIỆC & NÂNG CẤP (Phía Đông Bắc - Scale 0.35x)
            GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(3.0f, 0, halfD - 1.5f), Quaternion.Euler(0, 180, 0), roomRoot, 0.35f);
            SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(3.0f, 0, halfD - 2.4f), Quaternion.identity, roomRoot, 0.35f);
            
            // Laptop trên bàn
            GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(3.0f, 0.32f, halfD - 1.5f), Quaternion.Euler(0, 180, 0), roomRoot, 0.20f);
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

            // 6. 🗑️ GÓC THÙNG RÁC (Góc Đông Bắc sát tường - Scale 0.35x)
            GameObject trash = SpawnModel(modelsFolder, "cardboardBoxOpen.fbx", new Vector3(halfW - 1.8f, 0, halfD - 1.8f), Quaternion.Euler(0, -35, 0), roomRoot, 0.35f);
            if (trash != null)
            {
                EnsureCollider(trash);
                SearchableObject trashSearch = trash.AddComponent<SearchableObject>();
                trashSearch.containerName = "Trash Pile";
                trashSearch.searchDuration = 1.5f;
                trashSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/TrashLootTable.asset");
            }

            // 7. 🍳 KHU BẾP (Mép tường phía Đông - Scale 0.35x)
            GameObject kitchen = SpawnModel(modelsFolder, "kitchenCabinet.fbx", new Vector3(halfW - 1.2f, 0, 0.5f), Quaternion.Euler(0, -90, 0), roomRoot, 0.35f);
            SpawnModel(modelsFolder, "kitchenSink.fbx", new Vector3(halfW - 1.2f, 0, -1.0f), Quaternion.Euler(0, -90, 0), roomRoot, 0.35f);
            if (kitchen != null)
            {
                EnsureCollider(kitchen);
                SearchableObject kitchenSearch = kitchen.AddComponent<SearchableObject>();
                kitchenSearch.containerName = "Kitchen Cabinet";
                kitchenSearch.searchDuration = 2.0f;
                kitchenSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/KitchenLootTable.asset");
            }

            // 8. 🚪 TỦ QUẦN ÁO & GIÁ SÁCH (Mép tường phía Tây - Scale 0.35x)
            GameObject wardrobe = SpawnModel(modelsFolder, "bookcaseClosed.fbx", new Vector3(-halfW + 1.2f, 0, -0.5f), Quaternion.Euler(0, 90, 0), roomRoot, 0.35f);
            SpawnModel(modelsFolder, "bookcaseOpen.fbx", new Vector3(-halfW + 1.2f, 0, -2.0f), Quaternion.Euler(0, 90, 0), roomRoot, 0.35f);
            if (wardrobe != null)
            {
                EnsureCollider(wardrobe);
                SearchableObject wardrobeSearch = wardrobe.AddComponent<SearchableObject>();
                wardrobeSearch.containerName = "Wardrobe";
                wardrobeSearch.searchDuration = 3.0f;
                wardrobeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/WardrobeLootTable.asset");
            }

            // 9. 🔒 KÉT SẮT BÍ MẬT (Góc Tây Nam - Scale 0.35x)
            GameObject safe = SpawnModel(modelsFolder, "cardboardBoxClosed.fbx", new Vector3(-halfW + 2.0f, 0, -halfD + 2.0f), Quaternion.Euler(0, 45, 0), roomRoot, 0.35f);
            if (safe != null)
            {
                EnsureCollider(safe);
                SearchableObject safeSearch = safe.AddComponent<SearchableObject>();
                safeSearch.containerName = "Secret Safe";
                safeSearch.searchDuration = 4.0f;
                safeSearch.lootTable = AssetDatabase.LoadAssetAtPath<LootTable>("Assets/ScriptableObjects/LootTables/SecretSafeLootTable.asset");
            }

            // 10. 👤 QUẦY DEALER VE CHAI (Góc Đông Nam - Scale 0.35x)
            GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", new Vector3(halfW - 2.5f, 0, -halfD + 2.5f), Quaternion.Euler(0, -45, 0), roomRoot, 0.35f);
            if (dealerDesk != null)
            {
                EnsureCollider(dealerDesk);
                dealerDesk.AddComponent<DealerInteractable>();
            }

            // 11. Cấu hình Ánh Sáng Tự Nhiên Mặt Trời Chiếu Xiên Qua Cửa Sổ
            SetupNaturalSunlight(roomRoot, -halfD, wallHeight);

            // 12. Đặt Player vào tâm phòng
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1.0f, 0);
            }

            Debug.Log($"<color=green>[RoomSceneBuilder] Successfully built {roomTitle} with Window, Door, and Ceiling Light!</color>");
        }

        private static void CreateRoomShellWithWindow(GameObject parent, float width, float depth, float height, string urbanFolder)
        {
            float halfW = width / 2f;
            float halfD = depth / 2f;
            float halfH = height / 2f;

            // Sàn nhà dày
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Room_Floor";
            floor.transform.SetParent(parent.transform, false);
            floor.transform.localPosition = new Vector3(0, -0.15f, 0);
            floor.transform.localScale = new Vector3(width, 0.3f, depth);
            ApplyMaterial(floor, "Mat_Room_Floor", new Color(0.32f, 0.25f, 0.20f));

            // Tường Bắc (Phía sau bàn làm việc & giường)
            CreateWall("Wall_North", "Mat_Room_Wall", new Vector3(0, halfH, halfD), new Vector3(width, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));

            // Tường Tây & Đông
            CreateWall("Wall_West", "Mat_Room_Wall", new Vector3(-halfW, halfH, 0), new Vector3(0.3f, height, depth), parent, new Color(0.82f, 0.80f, 0.76f));
            CreateWall("Wall_East", "Mat_Room_Wall", new Vector3(halfW, halfH, 0), new Vector3(0.3f, height, depth), parent, new Color(0.82f, 0.80f, 0.76f));

            // TƯỜNG NAM (KHOÉT KHUNG CỬA SỔ LỚN ĐÓN NẮNG)
            float windowWidth = 6.0f;
            float windowHeight = 2.4f;
            float windowBottomY = 1.0f;

            // Phần tường dưới cửa sổ
            CreateWall("Wall_South_Bottom", "Mat_Room_Wall", new Vector3(0, windowBottomY / 2f, -halfD), new Vector3(width, windowBottomY, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            // Phần tường trên cửa sổ
            float topWallHeight = height - (windowBottomY + windowHeight);
            CreateWall("Wall_South_Top", "Mat_Room_Wall", new Vector3(0, height - (topWallHeight / 2f), -halfD), new Vector3(width, topWallHeight, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            // Phần tường 2 bên cửa sổ
            float sideWallWidth = (width - windowWidth) / 2f;
            CreateWall("Wall_South_Left", "Mat_Room_Wall", new Vector3(-halfW + (sideWallWidth / 2f), windowBottomY + (windowHeight / 2f), -halfD), new Vector3(sideWallWidth, windowHeight, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            CreateWall("Wall_South_Right", "Mat_Room_Wall", new Vector3(halfW - (sideWallWidth / 2f), windowBottomY + (windowHeight / 2f), -halfD), new Vector3(sideWallWidth, windowHeight, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));

            // Kính Cửa Sổ Trong Suốt
            GameObject windowGlass = GameObject.CreatePrimitive(PrimitiveType.Cube);
            windowGlass.name = "Window_Glass_Pane";
            windowGlass.transform.SetParent(parent.transform, false);
            windowGlass.transform.localPosition = new Vector3(0, windowBottomY + (windowHeight / 2f), -halfD);
            windowGlass.transform.localScale = new Vector3(windowWidth, windowHeight, 0.04f);
            ApplyMaterial(windowGlass, "Mat_Window_Glass", new Color(0.80f, 0.92f, 1.0f, 0.3f));

            // Khung cửa sổ mỹ thuật
            GameObject windowFrame = SpawnModel(urbanFolder, "window-wide-type-a.fbx", new Vector3(0, windowBottomY + (windowHeight / 2f) - 0.2f, -halfD + 0.05f), Quaternion.identity, parent, 1.2f);
            if (windowFrame != null) EnsureCollider(windowFrame);

            // Trần nhà
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Room_Ceiling";
            ceiling.transform.SetParent(parent.transform, false);
            ceiling.transform.localPosition = new Vector3(0, height + 0.15f, 0);
            ceiling.transform.localScale = new Vector3(width, 0.3f, depth);
            ApplyMaterial(ceiling, "Mat_Room_Ceiling", new Color(0.92f, 0.92f, 0.92f));
        }

        private static void SetupNaturalSunlight(GameObject parent, float southZ, float wallHeight)
        {
            // Tìm Directional Light (Mặt trời) chính của Scene và chỉnh góc chiếu xiên qua cửa sổ
            Light sunLight = null;
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var l in allLights)
            {
                if (l.type == LightType.Directional)
                {
                    sunLight = l;
                    break;
                }
            }

            if (sunLight == null)
            {
                GameObject sunObj = new GameObject("Natural_Sunlight");
                sunLight = sunObj.AddComponent<Light>();
                sunLight.type = LightType.Directional;
            }

            // Góc chiếu xiên từ ngoài cửa sổ vào phòng (Tia nắng ấm áp ban ngày)
            sunLight.transform.rotation = Quaternion.Euler(28f, 25f, 0f);
            sunLight.color = new Color(1.0f, 0.95f, 0.84f); // Ánh nắng vàng ấm tự nhiên
            sunLight.intensity = 1.35f;
            sunLight.shadows = LightShadows.Soft;

            // Đèn viền hắt sáng từ cửa sổ (Window Glow Light)
            GameObject windowGlow = new GameObject("Window_Sun_Glow");
            windowGlow.transform.SetParent(parent.transform, false);
            windowGlow.transform.localPosition = new Vector3(0, 2.2f, southZ + 0.8f);

            Light glow = windowGlow.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1.0f, 0.92f, 0.80f);
            glow.intensity = 8.0f;
            glow.range = 14.0f;
            glow.shadows = LightShadows.None;
        }

        private static void CreateWall(string name, string matName, Vector3 pos, Vector3 size, GameObject parent, Color wallColor)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform, false);
            wall.transform.localPosition = pos;
            wall.transform.localScale = size;
            ApplyMaterial(wall, matName, wallColor);
        }

        private static void ApplyMaterial(GameObject obj, string matName, Color color)
        {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                string folder = "Assets/Materials/Environment";
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

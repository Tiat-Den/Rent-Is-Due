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
            if (bed != null)
            {
                EnsureCollider(bed);
                bed.AddComponent<RentIsDue.Gameplay.BedInteractable>(); // 😴 Ngủ để kết thúc ngày
            }



            // 5. 💻 BÀN LÀM VIỆC & NÂNG CẤP (Phía Đông Bắc - Scale 0.35x)
            GameObject desk = SpawnModel(modelsFolder, "desk.fbx", new Vector3(3.0f, 0, halfD - 1.5f), Quaternion.Euler(0, 180, 0), roomRoot, 0.35f);
            SpawnModel(modelsFolder, "chairDesk.fbx", new Vector3(3.0f, 0, halfD - 2.4f), Quaternion.identity, roomRoot, 0.35f);
            
            // Laptop trên bàn
            GameObject laptop = SpawnModel(modelsFolder, "laptop.fbx", new Vector3(3.0f, 0.32f, halfD - 1.5f), Quaternion.Euler(0, 180, 0), roomRoot, 0.20f);
            if (laptop != null)
            {
                EnsureCollider(laptop);

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

            // 10. ALLEYWAY (Khu Phố hẻm)
            GameObject alleyRoot = new GameObject("Alleyway_Environment");
            alleyRoot.transform.SetParent(roomRoot.transform, false);

            // Street Ambience Audio
            AudioSource alleyAudio = alleyRoot.AddComponent<AudioSource>();
            alleyAudio.spatialBlend = 1.0f; // 3D sound
            alleyAudio.rolloffMode = AudioRolloffMode.Linear;
            alleyAudio.minDistance = 5f;
            alleyAudio.maxDistance = 25f;
            alleyAudio.loop = true;
            alleyAudio.playOnAwake = true;
            alleyAudio.volume = 0.6f;
            
            // It will generate and play the street noise at runtime using a small helper script attached to it.
            alleyRoot.AddComponent<RentIsDue.Audio.StreetAmbiencePlayer>();

            // === SÀN HẺM & TƯỜNG (MODULAR) ===
            float zStart = halfD;
            float alleyLength = 16f;
            float zEnd = zStart + alleyLength;
            float urbanScale = 4f; 
            float step = 2f; 

            for (float z = zStart; z <= zEnd; z += step)
            {
                // Đường và vỉa hè
                SpawnModel(urbanFolder, "road-asphalt-straight.fbx", new Vector3(0, 0, z), Quaternion.identity, alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "road-asphalt-pavement.fbx", new Vector3(-2f, 0, z), Quaternion.identity, alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "road-asphalt-pavement.fbx", new Vector3(2f, 0, z), Quaternion.Euler(0, 180, 0), alleyRoot, urbanScale);
                
                // Tường trái (X = -4)
                SpawnModel(urbanFolder, "wall-a-painted.fbx", new Vector3(-4f, 0, z), Quaternion.Euler(0, 90, 0), alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "wall-a-window.fbx", new Vector3(-4f, 2f, z), Quaternion.Euler(0, 90, 0), alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "wall-a.fbx", new Vector3(-4f, 4f, z), Quaternion.Euler(0, 90, 0), alleyRoot, urbanScale);
                
                // Tường phải (X = 4)
                SpawnModel(urbanFolder, "wall-b-garage.fbx", new Vector3(4f, 0, z), Quaternion.Euler(0, -90, 0), alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "wall-a-window.fbx", new Vector3(4f, 2f, z), Quaternion.Euler(0, -90, 0), alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "wall-a.fbx", new Vector3(4f, 4f, z), Quaternion.Euler(0, -90, 0), alleyRoot, urbanScale);
            }

            // Bít 2 đầu hẻm
            for (float x = -2f; x <= 2f; x += 2f)
            {
                SpawnModel(urbanFolder, "wall-fence.fbx", new Vector3(x, 0, zEnd + 2f), Quaternion.identity, alleyRoot, urbanScale);
                SpawnModel(urbanFolder, "wall-broken-type-a.fbx", new Vector3(x, 0, zStart - 2f), Quaternion.Euler(0, 180, 0), alleyRoot, urbanScale);
            }

            // Trần che bớt sáng
            GameObject alleyCeiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            alleyCeiling.name = "Alley_Ceiling";
            alleyCeiling.transform.SetParent(alleyRoot.transform, false);
            alleyCeiling.transform.localPosition = new Vector3(0, 8f, zStart + 8f);
            alleyCeiling.transform.localScale = new Vector3(1f, 1f, 2f);
            alleyCeiling.transform.localRotation = Quaternion.Euler(180, 0, 0);
            ApplyMaterial(alleyCeiling, "Mat_AlleyFloor", new Color(0.05f, 0.05f, 0.05f));

            // === TRANG TRÍ FBX ===
            SpawnModel(urbanFolder, "detail-dumpster-open.fbx",   new Vector3(2.5f,  0, zStart + 10f),   Quaternion.Euler(0, -20, 0), alleyRoot, 3f);
            SpawnModel(urbanFolder, "detail-dumpster-closed.fbx", new Vector3(2.5f,  0, zStart + 12f), Quaternion.Euler(0, -5,  0), alleyRoot, 3f);
            SpawnModel(urbanFolder, "pallet.fbx",                 new Vector3(-2.5f, 0, zStart + 10f),   Quaternion.Euler(0,  45, 0), alleyRoot, 2.5f);
            SpawnModel(urbanFolder, "pallet-small.fbx",           new Vector3(-3f, 0.3f, zStart + 10.2f), Quaternion.Euler(0, 30, 0), alleyRoot, 2.5f);
            SpawnModel(urbanFolder, "detail-bench.fbx",           new Vector3(-2.5f, 0, zStart + 2f),   Quaternion.Euler(0, 180, 0), alleyRoot, 3f);
            SpawnModel(urbanFolder, "detail-awning-wide.fbx",     new Vector3(-3.5f, 2.2f, zStart + 11f), Quaternion.Euler(0, 90, 0), alleyRoot, 2.5f);
            SpawnModel(urbanFolder, "detail-awning-wide.fbx",     new Vector3(3.5f,  2.2f, zStart + 11f), Quaternion.Euler(0, -90, 0), alleyRoot, 2.5f);
            SpawnModel(urbanFolder, "detail-cables-type-a.fbx",   new Vector3(0,   4f,   zStart + 4f), Quaternion.Euler(0, 90, 0), alleyRoot, 3.0f);
            SpawnModel(urbanFolder, "scaffolding-structure.fbx",  new Vector3(-3f, 0, zStart + 14f), Quaternion.Euler(0, 90, 0), alleyRoot, 3.0f);

            // === ĐÈN ĐƯỜNG ===
            GameObject lamp1 = SpawnModel(urbanFolder, "detail-light-single.fbx", new Vector3(3f, 0, zStart + 6f), Quaternion.Euler(0, -90, 0), alleyRoot, 3f);
            GameObject lamp2 = SpawnModel(urbanFolder, "detail-light-single.fbx", new Vector3(-3f, 0, zStart + 6f), Quaternion.Euler(0, 90, 0), alleyRoot, 3f);
            // DayNightCycle setup
            RentIsDue.Environment.DayNightCycle dnc = alleyRoot.AddComponent<RentIsDue.Environment.DayNightCycle>();
            
            if (lamp1 != null) {
                GameObject l1 = new GameObject("LightSource");
                l1.transform.SetParent(lamp1.transform, false);
                l1.transform.localPosition = new Vector3(0, 4.2f, 1.2f);
                Light pt1 = l1.AddComponent<Light>();
                pt1.type = LightType.Spot;
                pt1.color = new Color(1f, 0.8f, 0.5f);
                pt1.intensity = 50f; // Tăng sáng vì đèn ở trên cao hơn
                pt1.range = 30f;
                pt1.spotAngle = 110f;
                dnc.streetLamps.Add(pt1);
            }
            if (lamp2 != null) {
                GameObject l2 = new GameObject("LightSource");
                l2.transform.SetParent(lamp2.transform, false);
                l2.transform.localPosition = new Vector3(0, 4.2f, 1.2f);
                Light pt2 = l2.AddComponent<Light>();
                pt2.type = LightType.Spot;
                pt2.color = new Color(1f, 0.8f, 0.5f);
                pt2.intensity = 50f;
                pt2.range = 30f;
                pt2.spotAngle = 110f;
                dnc.streetLamps.Add(pt2);
            }

            SpawnModel(urbanFolder, "detail-barrier-type-a.fbx", new Vector3(2.5f, 0, zStart + 2f), Quaternion.Euler(0, 15, 0), alleyRoot, 2.0f);
            SpawnModel(urbanFolder, "tree-shrub.fbx", new Vector3(-3.5f, 0, zStart + 16f), Quaternion.identity, alleyRoot, 2.5f);
            SpawnModel(urbanFolder, "tree-shrub.fbx", new Vector3(3.5f, 0, zStart + 16f), Quaternion.identity, alleyRoot, 2.5f);
            
            // Cửa ra Khu Phố (nằm ở phòng)
            GameObject streetDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            streetDoor.name = "Street_Door";
            streetDoor.transform.SetParent(roomRoot.transform, false);
            streetDoor.transform.localPosition = new Vector3(0, 1.25f, halfD - 0.2f);
            streetDoor.transform.localScale = new Vector3(1.5f, 2.5f, 0.2f);
            ApplyMaterial(streetDoor, "Mat_StreetDoor", new Color(0.5f, 0.3f, 0.1f));
            streetDoor.AddComponent<RentIsDue.Gameplay.StreetDoorInteractable>();

            // Cửa từ Khu Phố về Phòng (đặt ép vào tường trái)
            GameObject alleyDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            alleyDoor.name = "Alley_Door";
            alleyDoor.transform.SetParent(alleyRoot.transform, false);
            alleyDoor.transform.localPosition = new Vector3(-3.8f, 1.25f, zStart + 2f);
            alleyDoor.transform.localScale = new Vector3(0.2f, 2.5f, 1.5f);
            ApplyMaterial(alleyDoor, "Mat_StreetDoor", new Color(0.5f, 0.3f, 0.1f));
            alleyDoor.AddComponent<RentIsDue.Gameplay.StreetDoorInteractable>();

            // 11. DEALER Ngoài hẻm
            GameObject dealerAnchor = new GameObject("Dealer_Anchor");
            dealerAnchor.transform.SetParent(alleyRoot.transform, false);
            dealerAnchor.transform.localPosition = new Vector3(2.5f, 0, zStart + 8f);
            
            GameObject dealerDesk = SpawnModel(modelsFolder, "bench.fbx", Vector3.zero, Quaternion.Euler(0, -45, 0), dealerAnchor, 0.35f);
            if (dealerDesk != null)
            {
                EnsureCollider(dealerDesk);
                dealerDesk.AddComponent<DealerInteractable>();
                dealerDesk.AddComponent<RentIsDue.Gameplay.DailyOrderManager>();
            }

            // 12. CỬA HÀNG ĐỒ NGHỀ (Tool Shop)
            GameObject toolShopAnchor = new GameObject("ToolShop_Anchor");
            toolShopAnchor.transform.SetParent(alleyRoot.transform, false);
            toolShopAnchor.transform.localPosition = new Vector3(-3f, 0.5f, zStart + 8f);
            
            GameObject toolShop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            toolShop.name = "Tool_Shop";
            toolShop.transform.SetParent(toolShopAnchor.transform, false);
            toolShop.transform.localPosition = Vector3.zero;
            toolShop.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            ApplyMaterial(toolShop, "Mat_ToolShop", new Color(0.8f, 0.4f, 0.1f));
            toolShop.AddComponent<RentIsDue.Shop.ToolShopInteractable>();
            toolShop.AddComponent<RentIsDue.Shop.ToolShopManager>();

            // 13. BÀN SỬA ĐỒ (Repair Bench)
            GameObject repairAnchor = new GameObject("Repair_Anchor");
            repairAnchor.transform.SetParent(alleyRoot.transform, false);
            repairAnchor.transform.localPosition = new Vector3(0, 0, zStart + 14f);
            
            GameObject repairBenchAlley = SpawnModel(modelsFolder, "tableCoffee.fbx", Vector3.zero, Quaternion.identity, repairAnchor, 0.35f);
            if (repairBenchAlley != null)
            {
                EnsureCollider(repairBenchAlley);
                repairBenchAlley.AddComponent<RentIsDue.Gameplay.RepairManager>();
            }

            // 12. Cấu hình Ánh Sáng Tự Nhiên Mặt Trời Chiếu Xiên Qua Cửa Sổ
            SetupNaturalSunlight(roomRoot, -halfD, wallHeight);

            // 12. Playtest Logger
            GameObject logger = new GameObject("Playtest_Logger");
            logger.AddComponent<PlaytestLogger>();
            logger.transform.SetParent(roomRoot.transform);

            // 13. Đặt Player vào tâm phòng
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                player.transform.position = new Vector3(0, 1.0f, 0);
            }

            // 13. CỬA NHÀ KHO (Trong phòng chính)
            GameObject storageDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            storageDoor.name = "Storage_Door";
            storageDoor.transform.SetParent(roomRoot.transform, false);
            storageDoor.transform.localPosition = new Vector3(-halfW + 0.1f, 1.0f, 0);
            storageDoor.transform.localScale = new Vector3(0.2f, 2.0f, 1.2f);
            ApplyMaterial(storageDoor, "Mat_Door", new Color(0.3f, 0.2f, 0.1f));
            storageDoor.AddComponent<RentIsDue.Gameplay.StorageDoorInteractable>();

            // 14. XÂY DỰNG NHÀ KHO (STORAGE ROOM) - Nằm cách xa phòng chính
            GameObject storageRoot = new GameObject("StorageRoom");
            storageRoot.transform.position = new Vector3(30f, 0, 0); // Đẩy ra xa 30m

            // Sàn nhà kho
            GameObject sFloor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            sFloor.transform.SetParent(storageRoot.transform, false);
            sFloor.transform.localScale = new Vector3(0.6f, 1f, 0.6f); // 6x6m
            ApplyMaterial(sFloor, "Mat_StorageFloor", new Color(0.2f, 0.2f, 0.2f));

            // Tường nhà kho
            GameObject sWallN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sWallN.transform.SetParent(storageRoot.transform, false);
            sWallN.transform.localPosition = new Vector3(0, 1.5f, 3f);
            sWallN.transform.localScale = new Vector3(6f, 3f, 0.2f);
            ApplyMaterial(sWallN, "Mat_StorageWall", new Color(0.4f, 0.4f, 0.4f));

            GameObject sWallS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sWallS.transform.SetParent(storageRoot.transform, false);
            sWallS.transform.localPosition = new Vector3(0, 1.5f, -3f);
            sWallS.transform.localScale = new Vector3(6f, 3f, 0.2f);
            ApplyMaterial(sWallS, "Mat_StorageWall", new Color(0.4f, 0.4f, 0.4f));

            GameObject sWallE = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sWallE.transform.SetParent(storageRoot.transform, false);
            sWallE.transform.localPosition = new Vector3(3f, 1.5f, 0);
            sWallE.transform.localScale = new Vector3(0.2f, 3f, 6f);
            ApplyMaterial(sWallE, "Mat_StorageWall", new Color(0.4f, 0.4f, 0.4f));

            GameObject sWallW = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sWallW.transform.SetParent(storageRoot.transform, false);
            sWallW.transform.localPosition = new Vector3(-3f, 1.5f, 0);
            sWallW.transform.localScale = new Vector3(0.2f, 3f, 6f);
            ApplyMaterial(sWallW, "Mat_StorageWall", new Color(0.4f, 0.4f, 0.4f));

            // Cửa ra của nhà kho
            GameObject sExitDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sExitDoor.name = "Storage_Exit_Door";
            sExitDoor.transform.SetParent(storageRoot.transform, false);
            sExitDoor.transform.localPosition = new Vector3(-2.9f, 1.0f, 0);
            sExitDoor.transform.localScale = new Vector3(0.2f, 2.0f, 1.2f);
            ApplyMaterial(sExitDoor, "Mat_Door", new Color(0.3f, 0.2f, 0.1f));
            sExitDoor.AddComponent<RentIsDue.Gameplay.StorageExitInteractable>();

            // Điểm Spawn trong Nhà Kho
            GameObject sSpawn = new GameObject("StorageSpawnPoint");
            sSpawn.transform.SetParent(storageRoot.transform, false);
            sSpawn.transform.localPosition = new Vector3(-2.0f, 1.0f, 0);

            // Điểm Spawn chính (tạo nếu chưa có)
            GameObject mSpawn = GameObject.Find("PlayerSpawnPoint");
            if (mSpawn == null)
            {
                mSpawn = new GameObject("PlayerSpawnPoint");
                mSpawn.transform.position = new Vector3(0, 1.5f, 0);
            }

            // Spawner trong Nhà kho (Loot xịn hơn)
            GameObject sSpawner = new GameObject("Storage_LootSpawner");
            sSpawner.transform.SetParent(storageRoot.transform, false);
            sSpawner.transform.localPosition = Vector3.zero;
            var sLoot = sSpawner.AddComponent<RentIsDue.Loot.RandomFloorLootSpawner>();
            sLoot.spawnAreaSize = new Vector2(5f, 5f);
            sLoot.minItemsPerDay = 5;
            sLoot.maxItemsPerDay = 8;
            var table = UnityEditor.AssetDatabase.LoadAssetAtPath<RentIsDue.Loot.LootTable>("Assets/Resources/Loot/MainLootTable.asset");
            if (table != null) sLoot.floorLootTable = table;

            Debug.Log($"<color=green>[RoomSceneBuilder] Successfully built {roomTitle} with Window, Door, Ceiling Light, and Storage Unit!</color>");
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

            // Tường Bắc (Có khoét khung cửa ra hẻm)
            float doorW = 1.5f;
            float doorH = 2.5f;

            // Phần tường trên cửa
            float topH = height - doorH;
            CreateWall("Wall_North_Top", "Mat_Room_Wall", new Vector3(0, height - (topH / 2f), halfD), new Vector3(doorW, topH, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));

            // Phần tường 2 bên cửa
            float sideW = (width - doorW) / 2f;
            CreateWall("Wall_North_Left", "Mat_Room_Wall", new Vector3(-halfW + (sideW / 2f), halfH, halfD), new Vector3(sideW, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));
            CreateWall("Wall_North_Right", "Mat_Room_Wall", new Vector3(halfW - (sideW / 2f), halfH, halfD), new Vector3(sideW, height, 0.3f), parent, new Color(0.88f, 0.86f, 0.82f));

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
            Light[] allLights = Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude);
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

        private static GameObject SpawnModel(string folder, string fileName, Vector3 pos, Quaternion rot, GameObject parent, Vector3 scaleV3)
        {
            return SpawnModelInternal(folder, fileName, pos, rot, parent, scaleV3);
        }

        private static GameObject SpawnModel(string folder, string fileName, Vector3 pos, Quaternion rot, GameObject parent, float scale = 1f)
        {
            return SpawnModelInternal(folder, fileName, pos, rot, parent, Vector3.one * scale);
        }

        private static GameObject SpawnModelInternal(string folder, string fileName, Vector3 pos, Quaternion rot, GameObject parent, Vector3 scale)
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
            instance.transform.localScale = scale;

            // Bỏ hiệu ứng LOD (Level Of Detail) gây biến dạng model khi zoom out
            LODGroup[] lodGroups = instance.GetComponentsInChildren<LODGroup>();
            foreach (var lg in lodGroups)
            {
                LOD[] lods = lg.GetLODs();
                for (int i = 1; i < lods.Length; i++) 
                {
                    foreach (Renderer r in lods[i].renderers) 
                    {
                        if (r != null) r.enabled = false; // Tắt hoàn toàn lưới của LOD thấp
                    }
                }
                if (lods.Length > 0)
                {
                    foreach (Renderer r in lods[0].renderers) 
                    {
                        if (r != null) r.enabled = true; // Đảm bảo LOD cao nhất luôn bật
                    }
                }
                Object.DestroyImmediate(lg); // Xóa component LODGroup để nó không can thiệp nữa
            }

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
            string itemsFolder = "Assets/Resources/Items";
            string modelsFolder = "Assets/Models/FBX format";

            var modelMap = new System.Collections.Generic.Dictionary<string, string>
            {
                { "item_aluminum_can", "plantSmall2.fbx" },
                { "item_antique_clock", "radio.fbx" },
                { "item_broken_laptop", "laptop.fbx" },
                { "item_broken_phone", "computerMouse.fbx" },
                { "item_cardboard_box", "cardboardBoxClosed.fbx" },
                { "item_collectible_card", "books.fbx" },
                { "item_digital_camera", "speakerSmall.fbx" },
                { "item_empty_jar", "plantSmall1.fbx" },
                { "item_frying_pan", "toaster.fbx" },
                { "item_game_controller", "computerMouse.fbx" },
                { "item_gold_watch", "speakerSmall.fbx" },
                { "item_headphones", "speakerSmall.fbx" },
                { "item_keyboard", "computerKeyboard.fbx" },
                { "item_magazine_bundle", "books.fbx" },
                { "item_mouse", "computerMouse.fbx" },
                { "item_old_book", "books.fbx" },
                { "item_old_clothes", "pillow.fbx" },
                { "item_old_console", "radio.fbx" },
                { "item_old_gpu", "laptop.fbx" },
                { "item_old_mug", "plantSmall3.fbx" },
                { "item_old_newspaper", "books.fbx" },
                { "item_plastic_bottle", "plantSmall2.fbx" },
                { "item_rare_coin_collection", "books.fbx" },
                { "item_rare_console", "televisionVintage.fbx" },
                { "item_rare_painting", "televisionVintage.fbx" },
                { "item_rubber_duck", "bear.fbx" },
                { "item_vintage_camera", "speakerSmall.fbx" },
                { "item_vintage_radio", "radio.fbx" },
                { "item_vinyl_player", "televisionVintage.fbx" },
                { "item_wall_clock", "speakerSmall.fbx" }
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

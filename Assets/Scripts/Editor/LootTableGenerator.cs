using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using RentIsDue.Inventory;
using RentIsDue.Loot;

namespace RentIsDue.Editor
{
    public class LootTableGenerator
    {
        [MenuItem("Tools/Generate Categorized Loot Tables")]
        public static void GenerateLootTables()
        {
            string itemsFolder = "Assets/ScriptableObjects/Items";
            string lootFolder = "Assets/ScriptableObjects/LootTables";

            if (!Directory.Exists(lootFolder))
            {
                Directory.CreateDirectory(lootFolder);
            }

            // Load all ItemData assets
            Dictionary<string, ItemData> itemMap = new Dictionary<string, ItemData>();
            string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { itemsFolder });
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item != null && !string.IsNullOrEmpty(item.id))
                {
                    itemMap[item.id] = item;
                }
            }

            if (itemMap.Count == 0)
            {
                Debug.LogError("[LootTableGenerator] No ItemData assets found! Please run 'Tools -> Import Items from CSV' first.");
                return;
            }

            // 1. Trash Bin / Scrap Pile (Thùng rác / Phế liệu)
            CreateOrUpdateLootTable(lootFolder, "TrashLootTable", 20f, new (string id, float weight)[] {
                ("item_plastic_bottle", 50f),
                ("item_aluminum_can", 45f),
                ("item_old_newspaper", 40f),
                ("item_cardboard_box", 35f),
                ("item_empty_jar", 25f),
                ("item_rubber_duck", 15f),
                ("item_old_clothes", 10f)
            }, itemMap);

            // 2. Desk / Workspace (Bàn làm việc / Đồ điện tử)
            CreateOrUpdateLootTable(lootFolder, "DeskLootTable", 25f, new (string id, float weight)[] {
                ("item_mouse", 35f),
                ("item_keyboard", 30f),
                ("item_headphones", 25f),
                ("item_game_controller", 20f),
                ("item_broken_phone", 15f),
                ("item_old_gpu", 10f),
                ("item_broken_laptop", 8f),
                ("item_digital_camera", 5f)
            }, itemMap);

            // 3. Kitchen Cupboard (Tủ bếp / Nhà bếp)
            CreateOrUpdateLootTable(lootFolder, "KitchenLootTable", 25f, new (string id, float weight)[] {
                ("item_empty_jar", 40f),
                ("item_old_mug", 35f),
                ("item_frying_pan", 25f),
                ("item_plastic_bottle", 20f),
                ("item_aluminum_can", 20f)
            }, itemMap);

            // 4. Wardrobe / Bookshelf (Tủ quần áo / Kệ sách)
            CreateOrUpdateLootTable(lootFolder, "WardrobeLootTable", 20f, new (string id, float weight)[] {
                ("item_old_clothes", 45f),
                ("item_old_book", 35f),
                ("item_magazine_bundle", 30f),
                ("item_wall_clock", 20f),
                ("item_vintage_radio", 12f),
                ("item_vinyl_player", 6f),
                ("item_collectible_card", 8f)
            }, itemMap);

            // 5. Secret Safe / Locked Chest (Két sắt bí mật / Hộp kho báu)
            CreateOrUpdateLootTable(lootFolder, "SecretSafeLootTable", 10f, new (string id, float weight)[] {
                ("item_digital_camera", 25f),
                ("item_old_console", 25f),
                ("item_antique_clock", 20f),
                ("item_collectible_card", 20f),
                ("item_vintage_camera", 15f),
                ("item_rare_painting", 10f),
                ("item_gold_watch", 8f),
                ("item_rare_console", 6f),
                ("item_rare_coin_collection", 4f)
            }, itemMap);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[LootTableGenerator] Successfully created/updated 5 Categorized Loot Tables in {lootFolder}!</color>");
            EditorUtility.DisplayDialog("Loot Tables Generated", "Successfully generated 5 Categorized Loot Tables (Trash, Desk, Kitchen, Wardrobe, Safe)!", "OK");
        }

        private static void CreateOrUpdateLootTable(string folder, string tableName, float emptyWeight, (string id, float weight)[] dropDefs, Dictionary<string, ItemData> itemMap)
        {
            string assetPath = $"{folder}/{tableName}.asset";
            LootTable table = AssetDatabase.LoadAssetAtPath<LootTable>(assetPath);

            if (table == null)
            {
                table = ScriptableObject.CreateInstance<LootTable>();
                AssetDatabase.CreateAsset(table, assetPath);
            }

            table.emptyWeight = emptyWeight;
            table.drops = new List<LootDrop>();

            foreach (var def in dropDefs)
            {
                if (itemMap.TryGetValue(def.id, out ItemData item))
                {
                    table.drops.Add(new LootDrop { item = item, weight = def.weight });
                }
                else
                {
                    Debug.LogWarning($"[LootTableGenerator] Item with id '{def.id}' not found in itemMap!");
                }
            }

            EditorUtility.SetDirty(table);
        }
    }
}

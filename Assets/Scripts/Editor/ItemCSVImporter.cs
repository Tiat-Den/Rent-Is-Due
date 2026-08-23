using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using RentIsDue.Inventory;

namespace RentIsDue.Editor
{
    public class ItemCSVImporter
    {
        [MenuItem("Tools/Import Items from CSV")]
        public static void ImportItems()
        {
            // Tìm file CSV ở các đường dẫn khả dụng
            string[] possiblePaths = {
                Path.Combine(Application.dataPath, "../../Rent_Is_Due_Item_Database/items.csv"),
                Path.Combine(Application.dataPath, "../Rent_Is_Due_Item_Database/items.csv"),
                Path.Combine(Application.dataPath, "items.csv")
            };

            string csvPath = null;
            foreach (var p in possiblePaths)
            {
                if (File.Exists(p))
                {
                    csvPath = Path.GetFullPath(p);
                    break;
                }
            }

            if (string.IsNullOrEmpty(csvPath))
            {
                csvPath = EditorUtility.OpenFilePanel("Select items.csv", "", "csv");
            }

            if (string.IsNullOrEmpty(csvPath) || !File.Exists(csvPath))
            {
                Debug.LogError("[ItemCSVImporter] CSV file not found!");
                return;
            }

            string targetFolder = "Assets/ScriptableObjects/Items";
            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);
            }

            string[] lines = File.ReadAllLines(csvPath);
            if (lines.Length <= 1)
            {
                Debug.LogWarning("[ItemCSVImporter] CSV file is empty or only has headers.");
                return;
            }

            int count = 0;
            // Bỏ qua dòng header đầu tiên
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] fields = line.Split(',');
                if (fields.Length < 9) continue;

                string itemId = fields[0].Trim();
                string displayName = fields[1].Trim();
                string categoryStr = fields[2].Trim();
                string rarityStr = fields[3].Trim();
                float.TryParse(fields[4].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float baseValue);
                float.TryParse(fields[5].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float weightKg);
                float.TryParse(fields[6].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float searchTimeSec);
                int.TryParse(fields[7].Trim(), out int maxStack);
                float.TryParse(fields[8].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float collectorMultiplier);

                // Parse Enums
                if (!Enum.TryParse(categoryStr, true, out ItemCategory category))
                {
                    category = ItemCategory.Other;
                }

                if (!Enum.TryParse(rarityStr, true, out ItemRarity rarity))
                {
                    rarity = ItemRarity.Common;
                }

                string assetPath = $"{targetFolder}/{itemId}.asset";
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(assetPath);

                if (itemData == null)
                {
                    itemData = ScriptableObject.CreateInstance<ItemData>();
                    AssetDatabase.CreateAsset(itemData, assetPath);
                }

                itemData.id = itemId;
                itemData.displayName = displayName;
                itemData.category = category;
                itemData.rarity = rarity;
                itemData.baseValue = baseValue;
                itemData.weight = weightKg;
                itemData.searchTimeSec = searchTimeSec;
                itemData.maxStack = maxStack;
                itemData.collectorMultiplier = collectorMultiplier;

                EditorUtility.SetDirty(itemData);
                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[ItemCSVImporter] Successfully imported {count} items into {targetFolder}!</color>");
            EditorUtility.DisplayDialog("Item Import Complete", $"Successfully imported {count} items from items.csv!", "OK");
        }
    }
}

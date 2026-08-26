using System.IO;
using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Shop;
using RentIsDue.Inventory;
using RentIsDue.Player;

namespace RentIsDue.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SaveGame()
        {
            // Tránh việc người chơi Save/Thoát game khi đang sửa đồ gây mất tiền oan
            if (RentIsDue.Gameplay.RepairManager.Instance != null)
            {
                RentIsDue.Gameplay.RepairManager.Instance.CancelCurrentRepair();
            }

            SaveData data = new SaveData();
            
            if (DayManager.Instance != null)
                data.currentDay = DayManager.Instance.currentDay;
                
            if (EconomyManager.Instance != null)
                data.currentMoney = EconomyManager.Instance.currentMoney;

            if (DebtManager.Instance != null)
                data.currentDebt = DebtManager.Instance.currentDebt;
                
            if (UpgradeManager.Instance != null)
            {
                data.backpackLevel = UpgradeManager.Instance.backpackLevel;
                data.carryWeightLevel = UpgradeManager.Instance.carryWeightLevel;
                data.staminaLevel = UpgradeManager.Instance.staminaLevel;
                data.staminaRegenLevel = UpgradeManager.Instance.staminaRegenLevel;
                data.moveSpeedLevel = UpgradeManager.Instance.moveSpeedLevel;
                data.searchSpeedLevel = UpgradeManager.Instance.searchSpeedLevel;
                data.sellPriceLevel = UpgradeManager.Instance.sellPriceLevel;
                data.repairSpeedLevel = UpgradeManager.Instance.repairSpeedLevel;
                data.isStorageUnlocked = UpgradeManager.Instance.isStorageUnlocked;
            }

            if (InventoryManager.Instance != null)
            {
                foreach (var inst in InventoryManager.Instance.items)
                {
                    data.inventory.Add(new SavedItem { id = inst.data.id, condition = inst.condition });
                }
            }

            if (RentIsDue.Gameplay.CollectorManager.Instance != null)
            {
                foreach (var item in RentIsDue.Gameplay.CollectorManager.Instance.currentSet)
                {
                    data.currentCollectorSet.Add(item.id);
                }
            }

            string json = JsonUtility.ToJson(data, true);
            string path = Application.persistentDataPath + "/save.json";
            File.WriteAllText(path, json);
            Debug.Log("Game Saved to: " + path);
        }

        public void LoadGame()
        {
            string path = Application.persistentDataPath + "/save.json";
            if (!File.Exists(path))
            {
                Debug.Log("No save file found.");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    Debug.LogError("[SaveManager] Save file is corrupted or empty — aborting load.");
                    return;
                }

                if (EconomyManager.Instance != null)
                    EconomyManager.Instance.currentMoney = data.currentMoney;

                if (DebtManager.Instance != null)
                    DebtManager.Instance.currentDebt = data.currentDebt;

                if (DayManager.Instance != null)
                {
                    DayManager.Instance.currentDay = data.currentDay > 0 ? data.currentDay : 1;
                    DayManager.Instance.CalculateRent();
                }

                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.backpackLevel = data.backpackLevel > 0 ? data.backpackLevel : 1;
                    UpgradeManager.Instance.carryWeightLevel = data.carryWeightLevel > 0 ? data.carryWeightLevel : 1;
                    UpgradeManager.Instance.staminaLevel = data.staminaLevel > 0 ? data.staminaLevel : 1;
                    UpgradeManager.Instance.staminaRegenLevel = data.staminaRegenLevel > 0 ? data.staminaRegenLevel : 1;
                    UpgradeManager.Instance.moveSpeedLevel = data.moveSpeedLevel > 0 ? data.moveSpeedLevel : 1;
                    UpgradeManager.Instance.searchSpeedLevel = data.searchSpeedLevel > 0 ? data.searchSpeedLevel : 1;
                    UpgradeManager.Instance.sellPriceLevel = data.sellPriceLevel > 0 ? data.sellPriceLevel : 1;
                    UpgradeManager.Instance.repairSpeedLevel = data.repairSpeedLevel > 0 ? data.repairSpeedLevel : 1;
                    UpgradeManager.Instance.isStorageUnlocked = data.isStorageUnlocked;

                    UpgradeManager.Instance.ApplyAllUpgrades();
                }

                if (InventoryManager.Instance != null && data.inventory != null)
                {
                    InventoryManager.Instance.items.Clear();
                    
                    // Load từ Assets/Resources/Items
                    ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
                    System.Collections.Generic.Dictionary<string, ItemData> itemDict = new System.Collections.Generic.Dictionary<string, ItemData>();
                    foreach (var i in allItems) itemDict[i.id] = i;

                    foreach (var savedItem in data.inventory)
                    {
                        if (itemDict.TryGetValue(savedItem.id, out ItemData matchedData))
                        {
                            InventoryManager.Instance.AddItem(matchedData, savedItem.condition);
                        }
                    }
                }

                if (RentIsDue.Gameplay.CollectorManager.Instance != null && data.currentCollectorSet != null && data.currentCollectorSet.Count > 0)
                {
                    RentIsDue.Gameplay.CollectorManager.Instance.currentSet.Clear();
                    ItemData[] allItems = Resources.LoadAll<ItemData>("Items");
                    System.Collections.Generic.Dictionary<string, ItemData> itemDict = new System.Collections.Generic.Dictionary<string, ItemData>();
                    foreach (var i in allItems) itemDict[i.id] = i;

                    float totalValue = 0f;
                    foreach (var setId in data.currentCollectorSet)
                    {
                        if (itemDict.TryGetValue(setId, out ItemData matchedData))
                        {
                            RentIsDue.Gameplay.CollectorManager.Instance.currentSet.Add(matchedData);
                            totalValue += matchedData.baseValue;
                        }
                    }
                    RentIsDue.Gameplay.CollectorManager.Instance.currentReward = totalValue * 4f;
                }

                Debug.Log("Game Loaded from: " + path);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to load save file: {ex.Message}");
            }
        }
    }
}

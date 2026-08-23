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
            SaveData data = new SaveData();
            
            if (DayManager.Instance != null)
                data.currentDay = DayManager.Instance.currentDay;
                
            if (EconomyManager.Instance != null)
                data.currentMoney = EconomyManager.Instance.currentMoney;
                
            if (UpgradeManager.Instance != null)
            {
                data.backpackLevel = UpgradeManager.Instance.backpackLevel;
                data.carryWeightLevel = UpgradeManager.Instance.carryWeightLevel;
                data.staminaLevel = UpgradeManager.Instance.staminaLevel;
                data.moveSpeedLevel = UpgradeManager.Instance.moveSpeedLevel;
                data.searchSpeedLevel = UpgradeManager.Instance.searchSpeedLevel;
                data.sellPriceLevel = UpgradeManager.Instance.sellPriceLevel;
            }

            string json = JsonUtility.ToJson(data, true);
            string path = Application.persistentDataPath + "/save.json";
            File.WriteAllText(path, json);
            Debug.Log("Game Saved to: " + path);
        }

        public void LoadGame()
        {
            string path = Application.persistentDataPath + "/save.json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                if (DayManager.Instance != null)
                {
                    DayManager.Instance.currentDay = data.currentDay;
                    DayManager.Instance.CalculateRent();
                }

                if (EconomyManager.Instance != null)
                    EconomyManager.Instance.currentMoney = data.currentMoney;

                if (UpgradeManager.Instance != null)
                {
                    UpgradeManager.Instance.backpackLevel = data.backpackLevel > 0 ? data.backpackLevel : 1;
                    UpgradeManager.Instance.carryWeightLevel = data.carryWeightLevel > 0 ? data.carryWeightLevel : 1;
                    UpgradeManager.Instance.staminaLevel = data.staminaLevel > 0 ? data.staminaLevel : 1;
                    UpgradeManager.Instance.moveSpeedLevel = data.moveSpeedLevel > 0 ? data.moveSpeedLevel : 1;
                    UpgradeManager.Instance.searchSpeedLevel = data.searchSpeedLevel > 0 ? data.searchSpeedLevel : 1;
                    UpgradeManager.Instance.sellPriceLevel = data.sellPriceLevel > 0 ? data.sellPriceLevel : 1;

                    UpgradeManager.Instance.ApplyAllUpgrades();
                }

                Debug.Log("Game Loaded from: " + path);
            }
            else
            {
                Debug.Log("No save file found.");
            }
        }
    }
}

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
                data.movementLevel = UpgradeManager.Instance.movementLevel;
                data.searchSpeedLevel = UpgradeManager.Instance.searchSpeedLevel;
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
                    UpgradeManager.Instance.backpackLevel = data.backpackLevel;
                    UpgradeManager.Instance.movementLevel = data.movementLevel;
                    UpgradeManager.Instance.searchSpeedLevel = data.searchSpeedLevel;
                }

                // Re-apply upgrade effects
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.maxSlots = 8 + (data.backpackLevel - 1) * 4;
                    InventoryManager.Instance.maxWeight = 20f + (data.backpackLevel - 1) * 10f;
                }

                PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.moveSpeed = 5f * (1f + (data.movementLevel - 1) * 0.2f);
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

using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Inventory;
using RentIsDue.Player;

namespace RentIsDue.Shop
{
    public class UpgradeUI : MonoBehaviour
    {
        private bool isUIVisible = false;

        public void ToggleUI()
        {
            isUIVisible = !isUIVisible;
        }

        private void OnGUI()
        {
            if (!isUIVisible) return;

            GUI.Window(2, new Rect(Screen.width / 2 - 150, Screen.height / 2 - 100, 300, 200), UpgradeWindow, "Upgrade Shop");
        }

        private void UpgradeWindow(int windowID)
        {
            if (UpgradeManager.Instance == null || EconomyManager.Instance == null || InventoryManager.Instance == null)
            {
                GUILayout.Label("Missing Managers!");
                if (GUILayout.Button("Close"))
                {
                    ToggleUI();
                }
                return;
            }

            GUILayout.Label($"Current Money: ${EconomyManager.Instance.currentMoney}");

            // Backpack Upgrade
            int backpackCost = UpgradeManager.Instance.backpackLevel * 100;
            if (GUILayout.Button($"Upgrade Backpack (Lv {UpgradeManager.Instance.backpackLevel}) - ${backpackCost}"))
            {
                if (EconomyManager.Instance.currentMoney >= backpackCost)
                {
                    EconomyManager.Instance.currentMoney -= backpackCost;
                    UpgradeManager.Instance.backpackLevel++;
                    InventoryManager.Instance.maxSlots += 4;
                    InventoryManager.Instance.maxWeight += 5f;
                }
            }

            // Movement Upgrade
            int movementCost = UpgradeManager.Instance.movementLevel * 150;
            if (GUILayout.Button($"Upgrade Movement (Lv {UpgradeManager.Instance.movementLevel}) - ${movementCost}"))
            {
                if (EconomyManager.Instance.currentMoney >= movementCost)
                {
                    EconomyManager.Instance.currentMoney -= movementCost;
                    UpgradeManager.Instance.movementLevel++;
                    
                    PlayerMovement playerMovement = FindAnyObjectByType<PlayerMovement>();
                    if (playerMovement != null)
                    {
                        playerMovement.moveSpeed *= 1.1f;
                    }
                }
            }

            // Search Speed Upgrade
            int searchSpeedCost = UpgradeManager.Instance.searchSpeedLevel * 120;
            if (GUILayout.Button($"Upgrade Search Speed (Lv {UpgradeManager.Instance.searchSpeedLevel}) - ${searchSpeedCost}"))
            {
                if (EconomyManager.Instance.currentMoney >= searchSpeedCost)
                {
                    EconomyManager.Instance.currentMoney -= searchSpeedCost;
                    UpgradeManager.Instance.searchSpeedLevel++;
                }
            }

            if (GUILayout.Button("Close"))
            {
                ToggleUI();
            }

            GUI.DragWindow();
        }
    }
}

using UnityEngine;
using RentIsDue.Inventory;
using System.Collections.Generic;

namespace RentIsDue.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        public float currentMoney = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SellItem(ItemData item, float marketModifier = 1f)
        {
            if (item != null)
            {
                float sellMultiplier = RentIsDue.Shop.UpgradeManager.Instance != null ? RentIsDue.Shop.UpgradeManager.Instance.GetSellPriceMultiplier() : 1f;
                float amount = item.baseValue * marketModifier * sellMultiplier;
                currentMoney += amount;
                InventoryManager.Instance.RemoveItem(item);
                
                if (RentIsDue.Audio.AudioManager.Instance != null)
                {
                    RentIsDue.Audio.AudioManager.Instance.PlaySell();
                }

                if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                {
                    RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage($"+${amount:F1} ({item.displayName})", Color.yellow);
                }

                if (RentIsDue.Core.PlaytestLogger.Instance != null)
                {
                    RentIsDue.Core.PlaytestLogger.Instance.RecordFirstSale(amount);
                }

                Debug.Log($"+ ${amount:F1} (Sell Multiplier: {sellMultiplier}x)");
            }
        }

        public void SellAllItems()
        {
            if (InventoryManager.Instance == null) return;
            
            List<ItemData> itemsToSell = new List<ItemData>(InventoryManager.Instance.items);
            foreach (var item in itemsToSell)
            {
                SellItem(item);
            }
        }
    }
}

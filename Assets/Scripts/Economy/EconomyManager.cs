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
                float amount = item.baseValue * marketModifier;
                currentMoney += amount;
                InventoryManager.Instance.RemoveItem(item);
                Debug.Log($"+ ${amount}");
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

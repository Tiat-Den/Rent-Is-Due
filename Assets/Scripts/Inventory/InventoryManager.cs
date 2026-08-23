using System.Collections.Generic;
using UnityEngine;

namespace RentIsDue.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        private static InventoryManager _instance;
        public static InventoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<InventoryManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("InventoryManager");
                        _instance = go.AddComponent<InventoryManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        public int maxSlots = 8;
        public float maxWeight = 20f;

        public List<ItemData> items = new List<ItemData>();
        
        public delegate void OnInventoryChanged();
        public OnInventoryChanged onInventoryChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public float GetTotalWeight()
        {
            float total = 0f;
            foreach (var item in items)
            {
                total += item.weight;
            }
            return total;
        }

        public bool AddItem(ItemData item)
        {
            if (items.Count >= maxSlots)
            {
                Debug.Log("Inventory full! No empty slots.");
                return false;
            }

            if (GetTotalWeight() + item.weight > maxWeight)
            {
                Debug.Log("Inventory too heavy! Cannot carry.");
                return false;
            }

            items.Add(item);
            onInventoryChanged?.Invoke();
            return true;
        }

        public void RemoveItem(ItemData item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                onInventoryChanged?.Invoke();
            }
        }

        public void DropItem(ItemData item, Vector3 dropPosition)
        {
            if (items.Contains(item))
            {
                if (item.prefab != null)
                {
                    Instantiate(item.prefab, dropPosition, Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("Item prefab is null, spawning placeholder.");
                    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    placeholder.transform.position = dropPosition;
                    placeholder.transform.localScale = Vector3.one * 0.3f;
                    PickupInteractable pickup = placeholder.AddComponent<PickupInteractable>();
                    pickup.itemData = item;
                }
                
                RemoveItem(item);
            }
        }
    }
}

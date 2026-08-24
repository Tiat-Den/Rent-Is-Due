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

        public List<ItemInstance> items = new List<ItemInstance>();
        
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
                if (item?.data != null) total += item.data.weight;
            }
            return total;
        }

        public bool AddItem(ItemData itemData)
        {
            if (itemData == null) return false;
            
            if (items.Count >= maxSlots)
            {
                Debug.Log("Inventory full! No empty slots.");
                return false;
            }

            if (GetTotalWeight() + itemData.weight > maxWeight)
            {
                Debug.Log("Inventory too heavy! Cannot carry.");
                return false;
            }

            // Mặc định đồ nhặt được sẽ bị hỏng (condition = 0.5f) cần bảo dưỡng
            ItemInstance newInst = new ItemInstance(itemData, 0.5f);
            items.Add(newInst);
            
            onInventoryChanged?.Invoke();
            return true;
        }

        public void RemoveItem(ItemInstance instance)
        {
            if (instance != null && items.Contains(instance))
            {
                items.Remove(instance);
                onInventoryChanged?.Invoke();
            }
        }

        public void DropItem(ItemInstance instance, Vector3 dropPosition)
        {
            if (instance != null && instance.data != null && items.Contains(instance))
            {
                if (instance.data.prefab != null)
                {
                    GameObject itemObj = Instantiate(instance.data.prefab, dropPosition, Quaternion.identity);
                    itemObj.transform.localScale = Vector3.one * 1.5f;

                    Collider col = itemObj.GetComponentInChildren<Collider>();
                    if (col == null)
                    {
                        BoxCollider bc = itemObj.AddComponent<BoxCollider>();
                        bc.size = new Vector3(0.5f, 0.5f, 0.5f);
                    }

                    PickupInteractable pickup = itemObj.GetComponent<PickupInteractable>();
                    if (pickup == null) pickup = itemObj.AddComponent<PickupInteractable>();
                    pickup.itemData = instance.data;
                }
                else
                {
                    Debug.LogWarning("Item prefab is null, spawning placeholder.");
                    GameObject placeholder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    placeholder.transform.position = dropPosition;
                    placeholder.transform.localScale = Vector3.one * 0.3f;
                    PickupInteractable pickup = placeholder.AddComponent<PickupInteractable>();
                    pickup.itemData = instance.data; // Note: drops reset condition for now
                }
                
                RemoveItem(instance);
            }
        }
    }
}

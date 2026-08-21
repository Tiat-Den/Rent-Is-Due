using UnityEngine;

namespace RentIsDue.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        private InventoryManager inventoryManager;

        private void Start()
        {
            inventoryManager = InventoryManager.Instance;
            if (inventoryManager != null)
            {
                inventoryManager.onInventoryChanged += UpdateUI;
            }
        }

        private void OnDestroy()
        {
            if (inventoryManager != null)
            {
                inventoryManager.onInventoryChanged -= UpdateUI;
            }
        }

        private void UpdateUI()
        {
            Debug.Log("--- Inventory Updated ---");
            Debug.Log($"Weight: {inventoryManager.GetTotalWeight()}/{inventoryManager.maxWeight} kg | Slots: {inventoryManager.items.Count}/{inventoryManager.maxSlots}");
            foreach (var item in inventoryManager.items)
            {
                Debug.Log($"- {item.displayName} ({item.weight}kg)");
            }
            Debug.Log("-------------------------");
        }

        private void OnGUI()
        {
            if (inventoryManager == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUI.Box(new Rect(0, 0, 300, 400), "");
            
            GUILayout.Label($"Inventory ({inventoryManager.items.Count}/{inventoryManager.maxSlots})");
            GUILayout.Label($"Weight: {inventoryManager.GetTotalWeight():F1} / {inventoryManager.maxWeight:F1} kg");
            
            GUILayout.Space(10);

            for (int i = 0; i < inventoryManager.items.Count; i++)
            {
                var item = inventoryManager.items[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{item.displayName} - {item.weight}kg");
                
                if (GUILayout.Button("Drop", GUILayout.Width(50)))
                {
                    // Drop slightly in front of the player (or camera)
                    Vector3 dropPos = transform.position;
                    if (Camera.main != null)
                    {
                        dropPos = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
                    }
                    inventoryManager.DropItem(item, dropPos);
                    break; // break to avoid collection modified exception during GUI loop
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }
    }
}

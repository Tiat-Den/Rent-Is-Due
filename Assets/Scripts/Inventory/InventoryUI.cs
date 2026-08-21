using UnityEngine;
using UnityEngine.InputSystem;

namespace RentIsDue.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        private InventoryManager inventoryManager;
        private bool isUIVisible = true;

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

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
            {
                isUIVisible = !isUIVisible;
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
            if (inventoryManager == null || !isUIVisible) return;

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
                    // Drop near the player
                    Vector3 dropPos = transform.position;
                    GameObject player = GameObject.Find("Player");
                    if (player != null)
                    {
                        dropPos = player.transform.position + player.transform.forward * 1.5f + Vector3.up * 0.5f;
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

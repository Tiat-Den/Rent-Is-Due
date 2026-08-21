using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Inventory
{
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        public ItemData itemData;

        public bool CanInteract(PlayerInteractor player)
        {
            return itemData != null;
        }

        public string GetInteractionText()
        {
            if (itemData != null)
            {
                return $"Pick up {itemData.displayName} ({itemData.weight}kg)";
            }
            return "Pick up";
        }

        public void Interact(PlayerInteractor player)
        {
            if (itemData != null)
            {
                bool added = InventoryManager.Instance.AddItem(itemData);
                if (added)
                {
                    Debug.Log($"Picked up {itemData.displayName}");
                    Destroy(gameObject);
                }
            }
        }
    }
}

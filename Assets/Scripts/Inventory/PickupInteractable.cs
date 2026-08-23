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
                    if (RentIsDue.Audio.AudioManager.Instance != null)
                    {
                        RentIsDue.Audio.AudioManager.Instance.PlayPickup();
                    }

                    if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                    {
                        RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage($"+ {itemData.displayName} ({itemData.weight}kg)", Color.cyan);
                    }

                    if (RentIsDue.Core.PlaytestLogger.Instance != null)
                    {
                        RentIsDue.Core.PlaytestLogger.Instance.RecordFirstItem();
                    }

                    Debug.Log($"[PickupInteractable] Picked up {itemData.displayName}");
                    Destroy(gameObject);
                }
                else
                {
                    if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                    {
                        RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage("Inventory Full or Too Heavy!", Color.red, 1.8f);
                    }
                }
            }
            else
            {
                Debug.LogWarning("[PickupInteractable] itemData is null on this object, removing object.");
                Destroy(gameObject);
            }
        }
    }
}

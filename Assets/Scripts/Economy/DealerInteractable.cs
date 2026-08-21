using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Economy
{
    public class DealerInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return "Press to Sell All Items";
        }

        public void Interact(PlayerInteractor player)
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.SellAllItems();
            }
            else
            {
                Debug.LogWarning("EconomyManager instance not found!");
            }
        }
    }
}

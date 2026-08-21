using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Shop
{
    public class UpgradeInteractable : MonoBehaviour, IInteractable
    {
        public UpgradeUI upgradeUI;

        public bool CanInteract(PlayerInteractor player)
        {
            return upgradeUI != null;
        }

        public string GetInteractionText()
        {
            return "Open Upgrade Shop";
        }

        public void Interact(PlayerInteractor player)
        {
            if (upgradeUI != null)
            {
                upgradeUI.ToggleUI();
            }
        }
    }
}

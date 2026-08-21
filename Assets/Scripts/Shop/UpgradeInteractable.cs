using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Shop
{
    public class UpgradeInteractable : MonoBehaviour, IInteractable
    {
        public UpgradeUI upgradeUI;

        private void Start()
        {
            if (upgradeUI == null)
            {
                upgradeUI = FindAnyObjectByType<UpgradeUI>();
            }
        }

        public bool CanInteract(PlayerInteractor player)
        {
            if (upgradeUI == null) upgradeUI = FindAnyObjectByType<UpgradeUI>();
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

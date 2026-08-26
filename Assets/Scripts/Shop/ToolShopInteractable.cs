using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Shop
{
    public class ToolShopInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return "Cửa Hàng Đồ Nghề";
        }

        public void Interact(PlayerInteractor player)
        {
            if (ToolShopManager.Instance != null)
            {
                ToolShopManager.Instance.OpenUI();
            }
        }
    }
}

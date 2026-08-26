using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Gameplay
{
    public class CollectorInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return "Bảng Sưu Tập VIP";
        }

        public void Interact(PlayerInteractor player)
        {
            if (CollectorManager.Instance != null)
            {
                CollectorManager.Instance.OpenUI();
            }
        }
    }
}

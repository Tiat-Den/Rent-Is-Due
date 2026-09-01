using UnityEngine;
using RentIsDue.Player;
using RentIsDue.Core;

namespace RentIsDue.Gameplay
{
    public class StreetDoorInteractable : MonoBehaviour, IInteractable
    {
        private bool isOpen = false;

        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return isOpen ? "[Đóng cửa]" : "[Mở cửa ra Hẻm]";
        }

        public void Interact(PlayerInteractor player)
        {
            isOpen = !isOpen;
            
            if (isOpen)
            {
                // Mở cửa: Xoay đi 90 độ và tắt collider để đi qua dễ hơn (hoặc giữ collider nếu xoay chuẩn)
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                // Dịch sang mép tường để không chắn đường (vì tâm cube nằm ở giữa)
                transform.localPosition += new Vector3(-0.75f, 0, 0.75f);
            }
            else
            {
                // Đóng cửa
                transform.localRotation = Quaternion.identity;
                transform.localPosition -= new Vector3(-0.75f, 0, 0.75f);
            }
        }
    }
}

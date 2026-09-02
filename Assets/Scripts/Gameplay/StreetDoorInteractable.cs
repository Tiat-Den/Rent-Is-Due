using UnityEngine;
using RentIsDue.Player;
using RentIsDue.Core;

namespace RentIsDue.Gameplay
{
    public class StreetDoorInteractable : MonoBehaviour, IInteractable
    {
        private bool isOpen = false;

        private Vector3 closedPos;
        private Quaternion closedRot;
        private bool isInit = false;

        private void Awake()
        {
            closedPos = transform.localPosition;
            closedRot = transform.localRotation;
            isInit = true;
        }

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
            if (!isInit)
            {
                closedPos = transform.localPosition;
                closedRot = transform.localRotation;
                isInit = true;
            }

            isOpen = !isOpen;
            
            if (isOpen)
            {
                // Mở cửa: Xoay đi 90 độ và nép sát tường trái
                transform.localRotation = Quaternion.Euler(0, 90, 0);
                transform.localPosition = closedPos + new Vector3(-0.75f, 0, 0.75f);
            }
            else
            {
                // Đóng cửa: Trở về vị trí và góc ban đầu
                transform.localRotation = closedRot;
                transform.localPosition = closedPos;
            }
        }
    }
}

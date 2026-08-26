using UnityEngine;
using RentIsDue.Player;
using RentIsDue.Core;

namespace RentIsDue.Gameplay
{
    public class StorageExitInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return "[Quay Về Phòng Chính]";
        }

        public void Interact(PlayerInteractor player)
        {
            GameObject mainSpawn = GameObject.Find("PlayerSpawnPoint"); // Tên mặc định hoặc tạo mới
            if (mainSpawn != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    player.transform.position = mainSpawn.transform.position;
                    cc.enabled = true;
                }
            }
        }
    }
}

using UnityEngine;
using RentIsDue.Player;

namespace RentIsDue.Gameplay
{
    public class StreetDoorInteractable : MonoBehaviour, IInteractable
    {
        private Transform playerTransform;

        private void Start()
        {
            var p = FindAnyObjectByType<RentIsDue.Player.PlayerController>();
            if (p != null) playerTransform = p.transform;
        }

        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            // If player is in room (z near 0), text is 'Exit to Alley'. If in alley (z near 30), text is 'Enter Room'.
            if (playerTransform != null && playerTransform.position.z > 15f)
            {
                return "Vo PhA?ng (Enter Room)";
            }
            return "Ra Khu PhA^' (Exit to Alley)";
        }

        public void Interact(PlayerInteractor player)
        {
            if (playerTransform != null)
            {
                if (playerTransform.position.z > 15f)
                {
                    // Teleport to room
                    playerTransform.position = new Vector3(0, 1.5f, 5f);
                }
                else
                {
                    // Teleport to alley
                    playerTransform.position = new Vector3(0, 1.5f, 25f);
                }
            }
        }
    }
}

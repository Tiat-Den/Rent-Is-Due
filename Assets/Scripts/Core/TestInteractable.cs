using UnityEngine;
using RentIsDue.Player;

namespace RentIsDue.Core
{
    public class TestInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionText = "Search Box";

        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            return $"[E] {interactionText}";
        }

        public void Interact(PlayerInteractor player)
        {
            Debug.Log($"Interacted with {gameObject.name}");
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Core;

namespace RentIsDue.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private LayerMask interactableLayer = ~0; // ~0 means Everything
        
        private IInteractable currentInteractable;

        private void Update()
        {
            FindInteractable();

            if (currentInteractable != null)
            {
                // TODO: Update UI with currentInteractable.GetInteractionText()

                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    if (currentInteractable.CanInteract(this))
                    {
                        currentInteractable.Interact(this);
                    }
                }
            }
            else
            {
                // TODO: Hide UI
            }
        }

        private void FindInteractable()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
            
            if (colliders.Length > 0)
            {
                float closestDistance = float.MaxValue;
                IInteractable closestInteractable = null;

                foreach (var col in colliders)
                {
                    IInteractable interactable = col.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        float distance = Vector3.Distance(transform.position, col.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestInteractable = interactable;
                        }
                    }
                }

                currentInteractable = closestInteractable;
            }
            else
            {
                currentInteractable = null;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

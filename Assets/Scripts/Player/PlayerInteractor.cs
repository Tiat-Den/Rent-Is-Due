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
            // 1. Ưu tiên kiểm tra bằng Raycast/SphereCast từ giữa màn hình (tầm mắt camera)
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (Physics.SphereCast(ray, 0.2f, out RaycastHit hit, interactionRange, interactableLayer))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        currentInteractable = interactable;
                        return;
                    }
                }
            }

            // 2. Dự phòng: Quét xung quanh bằng OverlapSphere nếu không trỏ thẳng
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

        private void OnGUI()
        {
            // Vẽ tâm ngắm nhỏ (Crosshair) chính giữa màn hình cho góc nhìn thứ nhất
            if (Time.timeScale > 0f)
            {
                float size = 6f;
                float x = (Screen.width - size) / 2f;
                float y = (Screen.height - size) / 2f;
                
                Color originalColor = GUI.color;
                GUI.color = currentInteractable != null ? Color.green : new Color(1, 1, 1, 0.6f);
                GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
                
                // Hiển thị gợi ý phím [E] khi đang nhìn vào vật thể có thể tương tác
                if (currentInteractable != null)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 14;
                    style.normal.textColor = Color.white;
                    GUI.Label(new Rect(x - 100, y + 15, 200, 30), currentInteractable.GetInteractionText(), style);
                }
                
                GUI.color = originalColor;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Core;

namespace RentIsDue.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 3.2f;
        [SerializeField] private LayerMask interactableLayer = ~0; // ~0 means Everything
        
        private IInteractable currentInteractable;

        private void Update()
        {
            FindInteractable();

            if (currentInteractable != null)
            {
                if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                {
                    if (currentInteractable.CanInteract(this))
                    {
                        currentInteractable.Interact(this);
                    }
                }
            }
        }

        private void FindInteractable()
        {
            // 1. Ưu tiên kiểm tra bằng SphereCast từ giữa màn hình (tầm mắt camera)
            if (Camera.main != null)
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (Physics.SphereCast(ray, 0.25f, out RaycastHit hit, interactionRange, interactableLayer))
                {
                    IInteractable interactable = hit.collider.GetComponent<IInteractable>() 
                                              ?? hit.collider.GetComponentInParent<IInteractable>()
                                              ?? hit.collider.GetComponentInChildren<IInteractable>();
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
                    IInteractable interactable = col.GetComponent<IInteractable>() 
                                              ?? col.GetComponentInParent<IInteractable>()
                                              ?? col.GetComponentInChildren<IInteractable>();
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
                
                // Hiển thị gợi ý phím [E] và thanh tiến trình bới đồ
                if (currentInteractable != null)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontSize = 14;
                    style.fontStyle = FontStyle.Bold;
                    style.normal.textColor = Color.white;

                    string prompt = currentInteractable.GetInteractionText();
                    
                    // Nếu là SearchableObject đang bới đồ, vẽ thêm thanh tiến trình (Progress Bar)
                    if (currentInteractable is RentIsDue.Loot.SearchableObject searchable)
                    {
                        GUI.Label(new Rect(x - 150, y + 12, 300, 25), prompt, style);
                    }
                    else
                    {
                        GUI.Label(new Rect(x - 150, y + 15, 300, 25), $"[E] {prompt}", style);
                    }
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

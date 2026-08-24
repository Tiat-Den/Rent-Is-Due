using UnityEngine;
using UnityEngine.InputSystem;
using RentIsDue.Core;

namespace RentIsDue.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 4.2f;
        [SerializeField] private LayerMask interactableLayer = ~0; // ~0 means Everything
        
        private IInteractable currentInteractable;
        private bool eKeyWasDown = false;

        private void Awake()
        {
            // Ép buộc tầm tương tác tối thiểu 4.2m để dễ nhặt đồ từ tầm mắt cao
            if (interactionRange < 4.2f)
            {
                interactionRange = 4.2f;
            }
            interactableLayer = ~0;
        }

        private void Update()
        {
            FindInteractable();

            if (currentInteractable != null)
            {
                bool ePressedNow = false;
                if (Keyboard.current != null)
                {
                    bool isDown = Keyboard.current.eKey.isPressed;
                    if (Keyboard.current.eKey.wasPressedThisFrame || (isDown && !eKeyWasDown))
                    {
                        ePressedNow = true;
                    }
                    eKeyWasDown = isDown;
                }

                if (ePressedNow)
                {
                    if (currentInteractable.CanInteract(this))
                    {
                        currentInteractable.Interact(this);
                    }
                }
            }
            else
            {
                if (Keyboard.current != null)
                {
                    eKeyWasDown = Keyboard.current.eKey.isPressed;
                }
            }
        }

        private void FindInteractable()
        {
            Camera cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();

            // 1. Quét thẳng bằng Raycast & SphereCast từ tâm mắt camera (Bỏ qua collider của chính Player)
            if (cam != null)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                
                // Thử Raycast chính xác trước
                RaycastHit[] rayHits = Physics.RaycastAll(ray, interactionRange, interactableLayer);
                System.Array.Sort(rayHits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (var hit in rayHits)
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform) || hit.transform.root == transform.root)
                        continue; // Bỏ qua cơ thể Player

                    IInteractable interactable = hit.collider.GetComponent<IInteractable>() 
                                              ?? hit.collider.GetComponentInParent<IInteractable>()
                                              ?? hit.collider.GetComponentInChildren<IInteractable>();
                    if (interactable != null)
                    {
                        currentInteractable = interactable;
                        return;
                    }
                }

                // Nếu Raycast chưa trúng, dùng SphereCast hình cầu rộng 0.3m để dễ nhặt đồ nhỏ trên sàn
                RaycastHit[] sphereHits = Physics.SphereCastAll(ray, 0.3f, interactionRange, interactableLayer);
                System.Array.Sort(sphereHits, (a, b) => a.distance.CompareTo(b.distance));

                foreach (var hit in sphereHits)
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform) || hit.transform.root == transform.root)
                        continue;

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
                    if (col.transform == transform || col.transform.IsChildOf(transform) || col.transform.root == transform.root)
                        continue;

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
            // Nếu con trỏ chuột không bị khóa (đang dùng chuột bấm UI), ẩn tâm ngắm đi
            if (Time.timeScale > 0f && Cursor.lockState == CursorLockMode.Locked)
            {
                float size = 6f;
                float x = (Screen.width - size) / 2f;
                float y = (Screen.height - size) / 2f;
                
                Color originalColor = GUI.color;
                GUI.color = currentInteractable != null ? Color.green : new Color(1, 1, 1, 0.6f);
                GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
                
                // Hiển thị gợi ý phím [E] và thanh tiến trình bới đồ
                if (currentInteractable != null && currentInteractable.CanInteract(this))
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
                        GUI.Label(new Rect(x - 250, y + 12, 500, 50), prompt, style);
                    }
                    else
                    {
                        GUI.Label(new Rect(x - 250, y + 15, 500, 50), $"[E] {prompt}", style);
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

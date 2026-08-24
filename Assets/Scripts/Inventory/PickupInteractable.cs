using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;

namespace RentIsDue.Inventory
{
    public class PickupInteractable : MonoBehaviour, IInteractable
    {
        public ItemData itemData;

        private void Start()
        {
            // Tự động sinh ra hình dáng 3D nếu vật phẩm được đặt bằng tay (spawn lúc đầu)
            // mà chưa có MeshRenderer (ví dụ: chỉ là một GameObject trống)
            if (itemData != null && itemData.prefab != null)
            {
                if (GetComponentInChildren<MeshRenderer>() == null)
                {
                    GameObject visual = Instantiate(itemData.prefab, transform.position, transform.rotation, transform);
                    visual.transform.localScale = Vector3.one * 0.5f;

                    if (GetComponent<Collider>() == null && visual.GetComponentInChildren<Collider>() == null)
                    {
                        BoxCollider bc = gameObject.AddComponent<BoxCollider>();
                        bc.size = new Vector3(0.5f, 0.5f, 0.5f);
                    }
                }
            }
        }

        public bool CanInteract(PlayerInteractor player)
        {
            return itemData != null;
        }

        public string GetInteractionText()
        {
            if (itemData != null)
            {
                return $"Pick up {itemData.displayName} ({itemData.weight}kg)";
            }
            return "Pick up";
        }

        public void Interact(PlayerInteractor player)
        {
            if (itemData != null)
            {
                bool added = InventoryManager.Instance.AddItem(itemData);
                if (added)
                {
                    if (RentIsDue.Audio.AudioManager.Instance != null)
                    {
                        RentIsDue.Audio.AudioManager.Instance.PlayPickup();
                    }

                    if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                    {
                        RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage($"+ {itemData.displayName} ({itemData.weight}kg)", Color.cyan);
                    }

                    if (RentIsDue.Core.PlaytestLogger.Instance != null)
                    {
                        RentIsDue.Core.PlaytestLogger.Instance.RecordFirstItem();
                    }

                    Debug.Log($"[PickupInteractable] Picked up {itemData.displayName}");
                    Destroy(gameObject);
                }
                else
                {
                    if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                    {
                        RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage("Inventory Full or Too Heavy!", Color.red, 1.8f);
                    }
                }
            }
            else
            {
                Debug.LogWarning("[PickupInteractable] itemData is null on this object, removing object.");
                Destroy(gameObject);
            }
        }
    }
}

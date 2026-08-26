using UnityEngine;
using RentIsDue.Player;
using RentIsDue.Economy;
using RentIsDue.Shop;
using RentIsDue.Core;

namespace RentIsDue.Gameplay
{
    public class StorageDoorInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract(PlayerInteractor player)
        {
            return true;
        }

        public string GetInteractionText()
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.isStorageUnlocked)
            {
                return "[Vào Nhà Kho]";
            }
            return "Mở khóa Nhà Kho ()";
        }

        public void Interact(PlayerInteractor player)
        {
            if (UpgradeManager.Instance != null && UpgradeManager.Instance.isStorageUnlocked)
            {
                // Teleport player to Storage spawn
                GameObject storageSpawn = GameObject.Find("StorageSpawnPoint");
                if (storageSpawn != null)
                {
                    player.transform.position = storageSpawn.transform.position;
                    // Note: CharacterController might need to be temporarily disabled to teleport
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        cc.enabled = false;
                        player.transform.position = storageSpawn.transform.position;
                        cc.enabled = true;
                    }
                }
            }
            else
            {
                // Try buy
                if (EconomyManager.Instance != null && EconomyManager.Instance.currentMoney >= 2000f)
                {
                    EconomyManager.Instance.currentMoney -= 2000f;
                    if (UpgradeManager.Instance != null) UpgradeManager.Instance.isStorageUnlocked = true;
                    if (RentIsDue.Audio.AudioManager.Instance != null) RentIsDue.Audio.AudioManager.Instance.PlaySell();
                    if (RentIsDue.Core.FloatingFeedbackUI.Instance != null) RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage("Đã mở khóa Nhà Kho!", Color.green);
                }
                else
                {
                    if (RentIsDue.Core.FloatingFeedbackUI.Instance != null) RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage("Không đủ !", Color.red);
                }
            }
        }
    }
}

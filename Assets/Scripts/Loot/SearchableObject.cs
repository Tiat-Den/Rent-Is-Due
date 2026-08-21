using System.Collections;
using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;
using RentIsDue.Inventory;

namespace RentIsDue.Loot
{
    public class SearchableObject : MonoBehaviour, IInteractable
    {
        public LootTable lootTable;
        public float searchDuration = 2f;
        public Transform spawnPoint;

        private bool hasBeenSearched = false;
        private bool isSearching = false;

        private void Start()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded += ResetSearchable;
            }
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded -= ResetSearchable;
            }
        }

        private void ResetSearchable()
        {
            hasBeenSearched = false;
            isSearching = false;
        }

        public bool CanInteract(PlayerInteractor player)
        {
            return !hasBeenSearched && !isSearching && lootTable != null;
        }

        public string GetInteractionText()
        {
            if (isSearching) return "Searching...";
            return "Search";
        }

        public void Interact(PlayerInteractor player)
        {
            if (CanInteract(player))
            {
                StartCoroutine(SearchCoroutine(player));
            }
        }

        private IEnumerator SearchCoroutine(PlayerInteractor player)
        {
            isSearching = true;
            Debug.Log("Searching started...");
            
            if (RentIsDue.Shop.UpgradeManager.Instance != null)
            {
                yield return new WaitForSeconds(searchDuration * RentIsDue.Shop.UpgradeManager.Instance.SearchSpeedMultiplier);
            }
            else
            {
                yield return new WaitForSeconds(searchDuration);
            }

            isSearching = false;
            hasBeenSearched = true;

            ItemData foundItem = lootTable.RollLoot();

            if (foundItem != null)
            {
                Debug.Log($"Found item: {foundItem.displayName}");
                SpawnItem(foundItem);
            }
            else
            {
                Debug.Log("Found nothing.");
            }
        }

        private void SpawnItem(ItemData itemData)
        {
            Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position + Vector3.up * 0.5f;
            
            GameObject prefabToSpawn = itemData.prefab != null ? itemData.prefab : GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject itemObject;

            if (itemData.prefab != null)
            {
                itemObject = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            }
            else
            {
                itemObject = prefabToSpawn;
                itemObject.transform.position = spawnPos;
                itemObject.transform.localScale = Vector3.one * 0.3f;
                
                if (itemObject.GetComponent<Collider>() == null)
                {
                    itemObject.AddComponent<BoxCollider>();
                }
            }
            
            PickupInteractable pickup = itemObject.GetComponent<PickupInteractable>();
            if (pickup == null)
            {
                pickup = itemObject.AddComponent<PickupInteractable>();
            }
            pickup.itemData = itemData;
        }
    }
}

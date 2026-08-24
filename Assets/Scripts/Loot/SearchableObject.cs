using System.Collections;
using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;
using RentIsDue.Inventory;

namespace RentIsDue.Loot
{
    public class SearchableObject : MonoBehaviour, IInteractable
    {
        [Header("Container Settings")]
        public string containerName = "Container";
        public LootTable lootTable;
        public float searchDuration = 2f;
        public Transform spawnPoint;

        private bool hasBeenSearched = false;
        private bool isSearching = false;
        private float searchProgress = 0f;

        private void Start()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded += ResetSearchable;
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines(); // Prevent ghost coroutine spawning loot after disable
            isSearching = false;
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded -= ResetSearchable;
            }
        }

        private void ResetSearchable()
        {
            StopAllCoroutines();
            hasBeenSearched = false;
            isSearching = false;
            searchProgress = 0f;
        }

        public bool CanInteract(PlayerInteractor player)
        {
            return !hasBeenSearched && !isSearching && lootTable != null;
        }

        public string GetInteractionText()
        {
            if (hasBeenSearched) return $"{containerName} (Empty)";
            if (isSearching) return $"Searching {containerName}... {(int)(searchProgress * 100)}%";
            return $"Search {containerName}";
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
            searchProgress = 0f;
            Debug.Log($"Searching {containerName} started...");

            float actualDuration = searchDuration;
            if (RentIsDue.Shop.UpgradeManager.Instance != null)
            {
                actualDuration = searchDuration * RentIsDue.Shop.UpgradeManager.Instance.SearchSpeedMultiplier;
            }

            float elapsed = 0f;
            while (elapsed < actualDuration)
            {
                elapsed += Time.deltaTime;
                searchProgress = Mathf.Clamp01(elapsed / actualDuration);
                yield return null;
            }

            isSearching = false;
            hasBeenSearched = true;
            searchProgress = 1f;

            // Guard: lootTable might have been cleared externally
            if (lootTable == null)
            {
                Debug.LogWarning($"[SearchableObject] lootTable is null on '{containerName}' — cannot roll loot.");
                yield break;
            }

            ItemData foundItem = lootTable.RollLoot();

            if (foundItem != null)
            {
                Debug.Log($"Found item in {containerName}: {foundItem.displayName} (${foundItem.baseValue})");
                SpawnItem(foundItem);
            }
            else
            {
                Debug.Log($"Found nothing in {containerName}.");
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

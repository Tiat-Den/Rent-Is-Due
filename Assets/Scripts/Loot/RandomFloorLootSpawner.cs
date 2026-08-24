using System.Collections.Generic;
using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Inventory;

namespace RentIsDue.Loot
{
    public class RandomFloorLootSpawner : MonoBehaviour
    {
        [Header("Loot Configuration")]
        [Tooltip("Bảng Loot dùng để quay thưởng đồ rớt ngẫu nhiên trên sàn")]
        public LootTable floorLootTable;
        
        [Header("Spawn Settings")]
        public int minItemsPerDay = 6;
        public int maxItemsPerDay = 12;
        
        [Header("Spawn Area (Khu vực phòng)")]
        public Vector3 spawnCenter = Vector3.zero;
        public Vector2 spawnAreaSize = new Vector2(8f, 8f);
        public LayerMask groundLayer = ~0;

        private List<GameObject> currentSpawnedItems = new List<GameObject>();

        private void Start()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded += RespawnFloorLoot;
            }

            // Sinh đồ lần đầu tiên khi vào game
            SpawnDailyLoot();
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded -= RespawnFloorLoot;
            }
        }

        public void RespawnFloorLoot()
        {
            ClearExistingLoot();
            SpawnDailyLoot();
        }

        public void ClearExistingLoot()
        {
            for (int i = currentSpawnedItems.Count - 1; i >= 0; i--)
            {
                if (currentSpawnedItems[i] != null)
                {
                    Destroy(currentSpawnedItems[i]);
                }
            }
            currentSpawnedItems.Clear();
        }

        public void SpawnDailyLoot()
        {
            if (floorLootTable == null)
            {
                // Tự động tìm TrashLootTable nếu chưa gán
                LootTable[] tables = Resources.FindObjectsOfTypeAll<LootTable>();
                if (tables.Length > 0) floorLootTable = tables[0];
            }

            if (floorLootTable == null) return;

            int count = Random.Range(minItemsPerDay, maxItemsPerDay + 1);

            for (int i = 0; i < count; i++)
            {
                ItemData item = floorLootTable.RollLoot();
                if (item != null)
                {
                    Vector3 randomPos = GetRandomFloorPosition();
                    SpawnItemAt(item, randomPos);
                }
            }

            Debug.Log($"<color=green>[FloorLootSpawner] Spawned {currentSpawnedItems.Count} random floor items for the day.</color>");
        }

        private Vector3 GetRandomFloorPosition()
        {
            float rx = Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f);
            float rz = Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f);
            // Bắn tia raycast từ ngang người xuống (tránh đụng trần nhà vì trần nhà ở Y=4.0)
            Vector3 origin = transform.position + spawnCenter + new Vector3(rx, 3.8f, rz);

            // Bắn tia raycast từ trên trời xuống để đặt đồ chạm khít mặt đất
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, groundLayer))
            {
                return hit.point + Vector3.up * 0.15f;
            }

            return transform.position + spawnCenter + new Vector3(rx, 0.15f, rz);
        }

        private void SpawnItemAt(ItemData itemData, Vector3 position)
        {
            GameObject itemObj = null;

            if (itemData.prefab != null)
            {
                itemObj = Instantiate(itemData.prefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                itemObj.transform.localScale = Vector3.one * 1.5f;

                Collider col = itemObj.GetComponentInChildren<Collider>();
                if (col == null)
                {
                    BoxCollider bc = itemObj.AddComponent<BoxCollider>();
                    bc.size = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }
            else
            {
                itemObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                itemObj.transform.position = position;
                itemObj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                itemObj.transform.localScale = Vector3.one * 0.25f;
                if (itemObj.GetComponent<Collider>() == null)
                {
                    itemObj.AddComponent<BoxCollider>();
                }
            }

            PickupInteractable pickup = itemObj.GetComponent<PickupInteractable>();
            if (pickup == null)
            {
                pickup = itemObj.AddComponent<PickupInteractable>();
            }
            pickup.itemData = itemData;

            currentSpawnedItems.Add(itemObj);
        }

        private void OnDrawGizmosSelected()
        {
            // Vẽ khung màu xanh lá cây trong Scene để bạn dễ dàng căn chỉnh khu vực rớt đồ
            Gizmos.color = Color.green;
            Vector3 center = transform.position + spawnCenter;
            Gizmos.DrawWireCube(center, new Vector3(spawnAreaSize.x, 0.2f, spawnAreaSize.y));
        }
    }
}

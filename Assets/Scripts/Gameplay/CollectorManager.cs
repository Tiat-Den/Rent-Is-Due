using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Inventory;

namespace RentIsDue.Gameplay
{
    public class CollectorManager : MonoBehaviour
    {
        public static CollectorManager Instance { get; private set; }

        public List<ItemData> currentSet = new List<ItemData>();
        public float currentReward = 0f;
        public bool IsUIOpen { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            GenerateNewSet();
        }

        public void GenerateNewSet()
        {
            currentSet.Clear();
            ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

            var legendaries = allItems.Where(i => i.rarity == ItemRarity.Legendary).ToList();
            var others = allItems.Where(i => i.rarity != ItemRarity.Legendary).ToList();

            if (legendaries.Count == 0 || others.Count < 2) return;

            // 1 Đồ Legendary
            currentSet.Add(legendaries[Random.Range(0, legendaries.Count)]);
            // 2 Đồ ngẫu nhiên khác
            for (int i = 0; i < 2; i++)
            {
                currentSet.Add(others[Random.Range(0, others.Count)]);
            }

            // Tính tiền thưởng: Giá trị gốc x4 lần
            float totalValue = currentSet.Sum(i => i.baseValue);
            currentReward = totalValue * 4f;
        }

        public void OpenUI()
        {
            IsUIOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseUI()
        {
            IsUIOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private bool CheckHasItem(ItemData data, List<ItemInstance> availableItems, out ItemInstance foundInstance)
        {
            foundInstance = availableItems.FirstOrDefault(i => i.data == data && i.condition >= 0.99f);
            return foundInstance != null;
        }

        public void TrySubmitSet()
        {
            var inventory = InventoryManager.Instance;
            if (inventory == null) return;

            List<ItemInstance> itemsToConsume = new List<ItemInstance>();
            List<ItemInstance> tempAvailable = new List<ItemInstance>(inventory.items);

            bool canSubmit = true;
            foreach (var reqItem in currentSet)
            {
                if (CheckHasItem(reqItem, tempAvailable, out ItemInstance found))
                {
                    itemsToConsume.Add(found);
                    tempAvailable.Remove(found);
                }
                else
                {
                    canSubmit = false;
                    break;
                }
            }

            if (canSubmit)
            {
                foreach (var item in itemsToConsume)
                {
                    inventory.RemoveItem(item);
                }

                if (EconomyManager.Instance != null) EconomyManager.Instance.currentMoney += currentReward;
                if (RentIsDue.Audio.AudioManager.Instance != null) RentIsDue.Audio.AudioManager.Instance.PlaySell();
                if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                    RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage($"Đã bán Bộ Sưu Tập! (+${currentReward:F1})", Color.green);

                GenerateNewSet();
            }
        }

        private void OnGUI()
        {
            if (!IsUIOpen) return;

            float w = 450f, h = 350f;
            Rect windowRect = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - h / 2f, w, h);
            GUI.Window(9005, windowRect, DrawWindow, "💎 KHÁCH VIP - BỘ SƯU TẬP 💎");
        }

        private void DrawWindow(int id)
        {
            GUI.DrawTexture(new Rect(0, 0, 450, 350), Texture2D.blackTexture);
            
            GUILayout.Space(10);
            GUILayout.Label("Khách VIP đang tìm kiếm một bộ sưu tập nguyên vẹn (Đã sửa 100%).\nGom đủ bộ để nhận mức giá gấp 4 lần thị trường!", GUI.skin.label);
            GUILayout.Space(15);

            var inventory = InventoryManager.Instance;
            List<ItemInstance> tempAvailable = inventory != null ? new List<ItemInstance>(inventory.items) : new List<ItemInstance>();

            bool allReady = true;

            foreach (var reqItem in currentSet)
            {
                GUILayout.BeginHorizontal();
                bool hasItem = CheckHasItem(reqItem, tempAvailable, out ItemInstance found);
                if (hasItem)
                {
                    tempAvailable.Remove(found);
                    GUI.color = Color.green;
                    GUILayout.Label($"[V] {reqItem.displayName} ({reqItem.rarity})", GUILayout.Width(300));
                }
                else
                {
                    allReady = false;
                    GUI.color = Color.gray;
                    GUILayout.Label($"[ ] {reqItem.displayName} ({reqItem.rarity})", GUILayout.Width(300));
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.Space(15);
            GUI.color = Color.yellow;
            GUILayout.Label($"Phần thưởng: ${currentReward:F1}", new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold });
            GUI.color = Color.white;
            GUILayout.Space(15);

            GUI.enabled = allReady;
            if (GUILayout.Button(allReady ? "💰 GIAO HÀNG NGAY" : "Chưa Đủ Đồ / Hoặc Đồ Bị Hỏng", GUILayout.Height(40)))
            {
                TrySubmitSet();
            }
            GUI.enabled = true;

            GUILayout.Space(10);
            if (GUILayout.Button("❌ Đóng", GUILayout.Height(30))) CloseUI();
        }
    }
}

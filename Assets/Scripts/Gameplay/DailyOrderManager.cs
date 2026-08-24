using System.Collections.Generic;
using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;
using RentIsDue.Inventory;
using RentIsDue.Economy;
using RentIsDue.Audio;
using RentIsDue.Shop;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Represents one special order from the Dealer each day.
    /// </summary>
    [System.Serializable]
    public class DailyOrder
    {
        public ItemCategory requiredCategory;   // e.g. Electronics, Kitchen…
        public int requiredQuantity;             // e.g. 2 items
        public float rewardMultiplier;           // e.g. 2.5x the normal sell price
        public bool isCompleted;

        public string Description =>
            $"Cần {requiredQuantity}x [{requiredCategory}] → Thưởng x{rewardMultiplier:F1} giá bán";
    }

    /// <summary>
    /// Manages daily special orders from the Dealer.
    /// Generates a new order each morning. Player fulfills it by talking to the Dealer.
    /// Attach to the Dealer desk or a Manager GameObject.
    /// </summary>
    public class DailyOrderManager : MonoBehaviour, IInteractable
    {
        public static DailyOrderManager Instance { get; private set; }

        [Header("Order Generation")]
        [Tooltip("Possible item categories the dealer might request")]
        public ItemCategory[] possibleCategories = {
            ItemCategory.Electronics, ItemCategory.Kitchen,
            ItemCategory.Clothing, ItemCategory.Toy,
            ItemCategory.Gaming, ItemCategory.Antique,
            ItemCategory.Collectible
        };

        [Range(1, 5)] public int minQuantity = 1;
        [Range(1, 5)] public int maxQuantity = 3;
        [Range(1.5f, 4f)] public float minReward = 1.5f;
        [Range(1.5f, 4f)] public float maxReward = 3.0f;

        [Header("Current Order")]
        public DailyOrder currentOrder;

        private bool _uiOpen = false;
        public bool IsUIOpen => _uiOpen;

        // ─── Lifecycle ───────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            GenerateNewOrder(); // First order on game start

            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayEnded += OnDayEnded;
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayEnded -= OnDayEnded;
        }

        private void OnDayEnded()
        {
            // Generate fresh order for next day
            GenerateNewOrder();
        }

        // ─── Order Logic ─────────────────────────────────────────────────────────

        private void GenerateNewOrder()
        {
            if (possibleCategories == null || possibleCategories.Length == 0) return;

            currentOrder = new DailyOrder
            {
                requiredCategory = possibleCategories[Random.Range(0, possibleCategories.Length)],
                requiredQuantity  = Random.Range(minQuantity, maxQuantity + 1),
                rewardMultiplier  = Mathf.Round(Random.Range(minReward, maxReward) * 2f) / 2f, // round to 0.5
                isCompleted       = false
            };

            Debug.Log($"[DailyOrderManager] New order: {currentOrder.Description}");

            if (FloatingFeedbackUI.Instance != null)
                FloatingFeedbackUI.Instance.ShowMessage($"📋 Đơn hàng mới!\n{currentOrder.Description}", Color.cyan);
        }

        /// <summary>
        /// Check inventory and fulfill the order if requirements met.
        /// Returns bonus money earned.
        /// </summary>
        private float TryFulfillOrder()
        {
            if (currentOrder == null || currentOrder.isCompleted) return 0f;
            if (InventoryManager.Instance == null || EconomyManager.Instance == null) return 0f;

            var items = InventoryManager.Instance.items;
            var matching = new List<ItemData>();

            foreach (var item in items)
            {
                if (item != null && item.category == currentOrder.requiredCategory)
                    matching.Add(item);
                if (matching.Count >= currentOrder.requiredQuantity) break;
            }

            if (matching.Count < currentOrder.requiredQuantity)
                return 0f; // not enough items

            // Sell matched items at the bonus multiplier
            float bonusTotal = 0f;
            foreach (var item in matching)
            {
                float sellMultiplier = UpgradeManager.Instance != null
                    ? UpgradeManager.Instance.GetSellPriceMultiplier() : 1f;
                float bonusAmount = item.baseValue * currentOrder.rewardMultiplier * sellMultiplier;
                bonusTotal += bonusAmount;
                EconomyManager.Instance.currentMoney += bonusAmount;
                InventoryManager.Instance.RemoveItem(item);
                Debug.Log($"[DailyOrderManager] Sold '{item.displayName}' for ${bonusAmount:F1} (x{currentOrder.rewardMultiplier} order bonus)");
            }

            currentOrder.isCompleted = true;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySell();
            if (FloatingFeedbackUI.Instance != null)
                FloatingFeedbackUI.Instance.ShowMessage(
                    $"✅ Đơn hàng hoàn thành!\n+${bonusTotal:F1} (x{currentOrder.rewardMultiplier} bonus)", Color.yellow);

            return bonusTotal;
        }

        // ─── IInteractable ───────────────────────────────────────────────────────

        public bool CanInteract(PlayerInteractor player) => !_uiOpen;

        public string GetInteractionText()
        {
            if (currentOrder == null) return "Dealer";
            if (currentOrder.isCompleted) return "Dealer (Đơn hàng hôm nay đã xong ✅)";
            return $"Dealer — {currentOrder.Description}";
        }

        public void Interact(PlayerInteractor player)
        {
            _uiOpen = !_uiOpen;
            Cursor.lockState = _uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _uiOpen;
        }

        // ─── OnGUI ───────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (!_uiOpen) return;

            float w = 400f, h = 340f;
            Rect windowRect = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - h / 2f, w, h);
            GUI.Window(9003, windowRect, DrawDealerWindow, "🏪 Dealer — Đơn Đặt Hàng Hôm Nay");
        }

        private void DrawDealerWindow(int id)
        {
            GUILayout.Space(8);

            float money = EconomyManager.Instance != null ? EconomyManager.Instance.currentMoney : 0f;
            GUILayout.Label($"💰 Tiền của bạn: ${money:F1}");
            GUILayout.Space(8);

            if (currentOrder == null)
            {
                GUILayout.Label("Không có đơn hàng hôm nay.");
            }
            else
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label($"📋 ĐƠN HÀNG HÔM NAY:");
                GUILayout.Space(4);
                GUILayout.Label($"  Loại đồ: {currentOrder.requiredCategory}");
                GUILayout.Label($"  Số lượng: {currentOrder.requiredQuantity} vật phẩm");
                GUILayout.Label($"  Phần thưởng: x{currentOrder.rewardMultiplier:F1} giá bán thông thường");
                GUILayout.Space(4);

                if (currentOrder.isCompleted)
                {
                    GUILayout.Label("✅ Đơn hàng đã hoàn thành hôm nay!");
                }
                else
                {
                    // Count matching items in inventory
                    int count = 0;
                    if (InventoryManager.Instance != null)
                        foreach (var item in InventoryManager.Instance.items)
                            if (item != null && item.category == currentOrder.requiredCategory) count++;

                    GUILayout.Label($"  Đồ đang có: {count}/{currentOrder.requiredQuantity}");
                    GUILayout.Space(6);

                    GUI.enabled = count >= currentOrder.requiredQuantity;
                    if (GUILayout.Button($"✅ Giao hàng & nhận thưởng!", GUILayout.Height(32)))
                    {
                        float earned = TryFulfillOrder();
                        if (earned <= 0f && FloatingFeedbackUI.Instance != null)
                            FloatingFeedbackUI.Instance.ShowMessage("Không đủ đồ để giao!", Color.red);
                    }
                    GUI.enabled = true;
                }
                GUILayout.EndVertical();
            }

            GUILayout.Space(10);

            // Normal sell all button too
            if (GUILayout.Button("💰 Bán Tất Cả Đồ (Giá Thường)", GUILayout.Height(28)))
            {
                EconomyManager.Instance?.SellAllItems();
                CloseUI();
            }

            GUILayout.Space(4);
            if (GUILayout.Button("❌ Đóng", GUILayout.Height(28))) CloseUI();
        }

        private void CloseUI()
        {
            _uiOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

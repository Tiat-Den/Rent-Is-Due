using System.Collections.Generic;
using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;
using RentIsDue.Inventory;
using RentIsDue.Economy;
using RentIsDue.Audio;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Workbench interactable. Player opens a repair UI showing all damaged items
    /// in inventory with their repair cost. Spending money restores condition to 1.0,
    /// boosting the eventual sell price.
    /// </summary>
    public class RepairManager : MonoBehaviour, IInteractable
    {
        public static RepairManager Instance { get; private set; }

        [Header("Workbench Settings")]
        public string workbenchName = "Repair Workbench";

        private bool _uiOpen = false;
        public bool IsUIOpen => _uiOpen;
        private Vector2 _scrollPos;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── IInteractable ───────────────────────────────────────────────────────

        public bool CanInteract(PlayerInteractor player) => !_uiOpen;

        public string GetInteractionText() => "Mở Bàn Sửa Đồ";

        public void Interact(PlayerInteractor player)
        {
            _uiOpen = !_uiOpen;
            // Lock/unlock cursor
            Cursor.lockState = _uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = _uiOpen;
        }

        // ─── Repair Logic ────────────────────────────────────────────────────────

        private bool _isRepairing = false;
        private float _repairProgress = 0f;
        private ItemInstance _currentlyRepairingItem = null;

        private float GetBaseRepairDuration(RentIsDue.Inventory.ItemRarity rarity)
        {
            switch (rarity)
            {
                case RentIsDue.Inventory.ItemRarity.Common: return 8.0f;
                case RentIsDue.Inventory.ItemRarity.Uncommon: return 9.5f;
                case RentIsDue.Inventory.ItemRarity.Rare: return 11.0f;
                case RentIsDue.Inventory.ItemRarity.Epic: return 12.5f;
                case RentIsDue.Inventory.ItemRarity.Legendary: return 14.0f;
                default: return 8.0f;
            }
        }

        private System.Collections.IEnumerator RepairRoutine(ItemInstance item, float cost)
        {
            _isRepairing = true;
            _currentlyRepairingItem = item;
            _repairProgress = 0f;

            // Trừ tiền ngay lập tức
            if (EconomyManager.Instance != null)
                EconomyManager.Instance.currentMoney -= cost;

            // Tính toán thời gian dựa vào độ hiếm & kỹ năng Upgrade
            float baseTime = GetBaseRepairDuration(item.data.rarity);
            float speedMultiplier = RentIsDue.Shop.UpgradeManager.Instance != null ? RentIsDue.Shop.UpgradeManager.Instance.GetRepairSpeedMultiplier() : 1f;
            float actualDuration = baseTime / speedMultiplier;

            // Chờ quá trình sửa
            float elapsed = 0f;
            while (elapsed < actualDuration)
            {
                elapsed += Time.deltaTime;
                _repairProgress = Mathf.Clamp01(elapsed / actualDuration);
                yield return null;
            }

            // Hoàn thành
            item.condition = 1f;
            _isRepairing = false;
            _currentlyRepairingItem = null;
            _repairProgress = 1f;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySell();
            if (FloatingFeedbackUI.Instance != null)
                FloatingFeedbackUI.Instance.ShowMessage($"Đã sửa {item.data.displayName}! (-${cost:F1})", Color.green);

            Debug.Log($"[RepairManager] Repaired '{item.data.displayName}' for ${cost:F1}");
        }

        // ─── OnGUI ───────────────────────────────────────────────────────────────

        private void OnGUI()
        {
            if (!_uiOpen) return;

            float w = 420f, h = 480f;
            Rect windowRect = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - h / 2f, w, h);
            GUI.Window(9002, windowRect, DrawRepairWindow, $"🔧 {workbenchName}");
        }

        private void DrawRepairWindow(int id)
        {
            // Dùng DrawTexture đen đặc để đảm bảo 100% che được UI đằng sau
            GUI.DrawTexture(new Rect(0, 0, 420, 480), Texture2D.blackTexture);
            
            GUILayout.Space(8);

            if (InventoryManager.Instance == null)
            {
                GUILayout.Label("Không tìm thấy Inventory!");
                if (GUILayout.Button("Đóng")) CloseUI();
                return;
            }

            float money = EconomyManager.Instance != null ? EconomyManager.Instance.currentMoney : 0f;
            GUILayout.Label($"💰 Tiền hiện có: ${money:F1}");
            GUILayout.Space(6);

            var items = InventoryManager.Instance.items;
            if (items == null || items.Count == 0)
            {
                GUILayout.Label("Túi đồ trống rỗng — không có gì để sửa.");
            }
            else
            {
                GUILayout.Label($"Đồ trong túi ({items.Count} vật phẩm):");
                GUILayout.Space(4);

                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(320));
                foreach (var item in items)
                {
                    if (item == null || item.data == null) continue;
                    
                    float repairCost = item.RepairCost;
                    bool needsRepair = item.IsDamaged;

                    GUILayout.BeginHorizontal("box");
                    string conditionText = needsRepair ? $"Hỏng ({(item.condition*100):F0}%)" : "Tốt (100%)";
                    GUILayout.Label($"{item.data.displayName} | {conditionText} | Giá: ${item.EffectiveValue:F1}", GUILayout.Width(260));
                    
                    if (_isRepairing && _currentlyRepairingItem == item)
                    {
                        // Đang sửa món này -> Hiện %
                        GUILayout.Box($"Đang sửa... {(_repairProgress * 100):F0}%", GUILayout.Width(130));
                    }
                    else
                    {
                        GUI.enabled = needsRepair && !_isRepairing; // Disable nút nếu đang sửa món khác
                        if (GUILayout.Button(needsRepair ? $"Bảo dưỡng (${repairCost:F1})" : "Đã sửa", GUILayout.Width(130)))
                        {
                            if (EconomyManager.Instance != null && EconomyManager.Instance.currentMoney >= repairCost)
                            {
                                StartCoroutine(RepairRoutine(item, repairCost));
                            }
                            else
                            {
                                if (FloatingFeedbackUI.Instance != null)
                                    FloatingFeedbackUI.Instance.ShowMessage("Không đủ tiền!", Color.red);
                            }
                        }
                        GUI.enabled = true;
                    }
                    
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(8);
            
            // Khóa nút Đóng nếu đang sửa để tránh lỗi tắt UI ngang
            GUI.enabled = !_isRepairing;
            if (GUILayout.Button("❌ Đóng", GUILayout.Height(30))) CloseUI();
            GUI.enabled = true;
        }

        private void CloseUI()
        {
            _uiOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

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

        /// <summary>
        /// Repair a single item instance: deduct cost from EconomyManager, restore condition to 1.
        /// Returns true on success.
        /// </summary>
        public bool RepairItem(ItemInstance instance)
        {
            if (instance == null || !instance.IsDamaged) return false;
            if (EconomyManager.Instance == null) return false;

            float cost = instance.RepairCost;
            if (EconomyManager.Instance.currentMoney < cost)
            {
                if (FloatingFeedbackUI.Instance != null)
                    FloatingFeedbackUI.Instance.ShowMessage("Không đủ tiền để sửa!", Color.red);
                return false;
            }

            EconomyManager.Instance.currentMoney -= cost;
            instance.condition = 1f;

            if (AudioManager.Instance != null) AudioManager.Instance.PlaySell();
            if (FloatingFeedbackUI.Instance != null)
                FloatingFeedbackUI.Instance.ShowMessage($"Đã sửa {instance.data.displayName}! (-${cost:F1})", Color.green);

            Debug.Log($"[RepairManager] Repaired '{instance.data.displayName}' for ${cost:F1}");
            return true;
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
            // Background đặc để che chữ ở phía sau
            GUI.Box(new Rect(0, 0, 420, 480), "", new GUIStyle(GUI.skin.box) { normal = { background = Texture2D.whiteTexture } });
            
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
                    
                    GUI.enabled = needsRepair;
                    if (GUILayout.Button(needsRepair ? $"Bảo dưỡng (${repairCost:F1})" : "Đã sửa", GUILayout.Width(130)))
                    {
                        RepairItem(item);
                    }
                    GUI.enabled = true;
                    
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }

            GUILayout.Space(8);
            if (GUILayout.Button("❌ Đóng", GUILayout.Height(30))) CloseUI();
        }

        private void CloseUI()
        {
            _uiOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}

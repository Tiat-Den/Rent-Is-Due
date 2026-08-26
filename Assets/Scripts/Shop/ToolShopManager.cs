using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Inventory;

namespace RentIsDue.Shop
{
    public class ToolShopManager : MonoBehaviour
    {
        public static ToolShopManager Instance { get; private set; }
        public bool IsUIOpen { get; private set; }

        private void Awake()
        {
            Instance = this;
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

        private void OnGUI()
        {
            if (!IsUIOpen) return;

            float w = 400f, h = 300f;
            Rect windowRect = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - h / 2f, w, h);
            GUI.Window(9006, windowRect, DrawWindow, "🛒 CỬA HÀNG ĐỒ NGHỀ");
        }

        private void DrawWindow(int id)
        {
            GUI.DrawTexture(new Rect(0, 0, 400, 300), Texture2D.blackTexture);
            
            GUILayout.Space(10);
            float money = EconomyManager.Instance != null ? EconomyManager.Instance.currentMoney : 0f;
            GUILayout.Label($"💰 Tiền hiện có: ${money:F1}");
            GUILayout.Space(10);

            DrawToolRow("Tuốc nơ vít", "Tăng tốc độ sửa chữa (x1.5)", "item_screwdriver", 300f);
            DrawToolRow("Xà beng", "Tăng tốc độ lục lọi (x1.5)", "item_crowbar", 400f);
            DrawToolRow("Két sắt mini", "Giữ tiền an toàn khỏi kẻ trộm (100%)", "item_safe", 1000f);

            GUILayout.Space(20);
            if (GUILayout.Button("❌ Đóng cửa hàng", GUILayout.Height(30)))
            {
                CloseUI();
            }
        }

        private void DrawToolRow(string name, string desc, string itemId, float price)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.BeginVertical();
            GUILayout.Label(name, new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label(desc, new GUIStyle(GUI.skin.label) { fontSize = 11 });
            GUILayout.EndVertical();

            bool hasTool = false;
            if (InventoryManager.Instance != null)
            {
                foreach (var item in InventoryManager.Instance.items)
                {
                    if (item.data != null && item.data.id == itemId)
                    {
                        hasTool = true;
                        break;
                    }
                }
            }

            if (hasTool)
            {
                GUI.color = Color.green;
                GUILayout.Label("Đã sở hữu", GUILayout.Width(80));
                GUI.color = Color.white;
            }
            else
            {
                if (GUILayout.Button($"Mua (${price:F0})", GUILayout.Width(80), GUILayout.Height(30)))
                {
                    if (EconomyManager.Instance.currentMoney >= price)
                    {
                        ItemData toolAsset = Resources.Load<ItemData>($"Items/{itemId}");
                        if (toolAsset != null && InventoryManager.Instance.AddItem(toolAsset, 1f))
                        {
                            EconomyManager.Instance.currentMoney -= price;
                            if (RentIsDue.Audio.AudioManager.Instance != null) RentIsDue.Audio.AudioManager.Instance.PlaySell();
                            if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                                RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage($"Đã mua {name}!", Color.green);
                        }
                    }
                    else
                    {
                        if (RentIsDue.Core.FloatingFeedbackUI.Instance != null)
                            RentIsDue.Core.FloatingFeedbackUI.Instance.ShowMessage("Không đủ tiền!", Color.red);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

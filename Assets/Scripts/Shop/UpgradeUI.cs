using UnityEngine;
using RentIsDue.Economy;

namespace RentIsDue.Shop
{
    public class UpgradeUI : MonoBehaviour
    {
        public bool isUIVisible { get; private set; } = false;
        private Vector2 scrollPos;

        public void ToggleUI()
        {
            isUIVisible = !isUIVisible;
        }

        private void OnGUI()
        {
            if (!isUIVisible) return;

            float width = 450;
            float height = 500;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            GUI.Window(2, new Rect(x, y, width, height), UpgradeWindow, "Hardware & Skill Upgrades");
        }

        private void UpgradeWindow(int windowID)
        {
            if (UpgradeManager.Instance == null || EconomyManager.Instance == null)
            {
                GUILayout.Label("Missing Managers!");
                if (GUILayout.Button("Close")) ToggleUI();
                return;
            }

            UpgradeManager um = UpgradeManager.Instance;
            float money = EconomyManager.Instance.currentMoney;

            GUILayout.BeginHorizontal();
            GUILayout.Label($"💰 Balance: <b>${money:F1}</b>", GUILayout.Height(25));
            if (GUILayout.Button("✖ Close", GUILayout.Width(75), GUILayout.Height(25)))
            {
                ToggleUI();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(410));

            // 1. Bigger Backpack
            DrawUpgradeCard(
                "🎒 Bigger Backpack",
                um.backpackLevel,
                um.GetMaxSlots() + " Slots",
                um.backpackLevel < 5 ? UpgradeManager.BackpackSlots[um.backpackLevel] + " Slots" : "MAX",
                um.backpackLevel < 5 ? UpgradeManager.BackpackCosts[um.backpackLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.backpackLevel, UpgradeManager.BackpackCosts, null)
            );

            // 2. Stronger Bag (Carry Weight)
            DrawUpgradeCard(
                "🏋️ Stronger Bag (Weight)",
                um.carryWeightLevel,
                um.GetMaxWeight() + " kg",
                um.carryWeightLevel < 5 ? UpgradeManager.CarryWeights[um.carryWeightLevel] + " kg" : "MAX",
                um.carryWeightLevel < 5 ? UpgradeManager.CarryWeightCosts[um.carryWeightLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.carryWeightLevel, UpgradeManager.CarryWeightCosts, null)
            );

            // 3. Better Fitness (Max Stamina)
            DrawUpgradeCard(
                "⚡ Better Fitness (Max Stamina)",
                um.staminaLevel,
                um.GetMaxStamina() + " Max",
                um.staminaLevel < 5 ? UpgradeManager.StaminaValues[um.staminaLevel] + " Max" : "MAX",
                um.staminaLevel < 5 ? UpgradeManager.StaminaCosts[um.staminaLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.staminaLevel, UpgradeManager.StaminaCosts, null)
            );

            // 4. Fast Recovery (Stamina Regen)
            DrawUpgradeCard(
                "🫁 Fast Recovery (Stamina Regen)",
                um.staminaRegenLevel,
                $"{um.GetStaminaRegenRate():F1}/s",
                um.staminaRegenLevel < 5 ? $"{UpgradeManager.StaminaRegenValues[um.staminaRegenLevel]:F1}/s" : "MAX",
                um.staminaRegenLevel < 5 ? UpgradeManager.StaminaRegenCosts[um.staminaRegenLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.staminaRegenLevel, UpgradeManager.StaminaRegenCosts, null)
            );

            // 4. Better Shoes (Move Speed)
            DrawUpgradeCard(
                "👟 Better Shoes (Move Speed)",
                um.moveSpeedLevel,
                $"{(int)(um.GetMoveSpeedMultiplier() * 100)}%",
                um.moveSpeedLevel < 5 ? $"{(int)(UpgradeManager.MoveSpeedMultipliers[um.moveSpeedLevel] * 100)}%" : "MAX",
                um.moveSpeedLevel < 5 ? UpgradeManager.MoveSpeedCosts[um.moveSpeedLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.moveSpeedLevel, UpgradeManager.MoveSpeedCosts, null)
            );

            // 5. Quick Search (Search Speed)
            DrawUpgradeCard(
                "🔍 Quick Search (Search Speed)",
                um.searchSpeedLevel,
                $"{(int)(um.GetSearchSpeedMultiplier() * 100)}%",
                um.searchSpeedLevel < 5 ? $"{(int)(UpgradeManager.SearchSpeedMultipliers[um.searchSpeedLevel] * 100)}%" : "MAX",
                um.searchSpeedLevel < 5 ? UpgradeManager.SearchSpeedCosts[um.searchSpeedLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.searchSpeedLevel, UpgradeManager.SearchSpeedCosts, null)
            );

            // 6. Better Selling (Sell Price Multiplier)
            DrawUpgradeCard(
                "📈 Better Selling (Trading)",
                um.sellPriceLevel,
                $"{um.GetSellPriceMultiplier():F2}x",
                um.sellPriceLevel < 5 ? $"{UpgradeManager.SellPriceMultipliers[um.sellPriceLevel]:F2}x" : "MAX",
                um.sellPriceLevel < 5 ? UpgradeManager.SellPriceCosts[um.sellPriceLevel] : 0,
                money,
                () => um.TryUpgrade(ref um.sellPriceLevel, UpgradeManager.SellPriceCosts, null)
            );

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private void DrawUpgradeCard(string title, int level, string currentVal, string nextVal, int cost, float currentMoney, System.Action onUpgrade)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{title}</b>");
            GUILayout.Label($"Level {level}/5", GUILayout.Width(70));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (level < 5)
            {
                GUILayout.Label($"Effect: {currentVal} ➔ <b>{nextVal}</b>");
                
                if (currentMoney >= cost)
                {
                    if (GUILayout.Button($"[ UPGRADE ${cost} ]", GUILayout.Width(130), GUILayout.Height(26)))
                    {
                        onUpgrade?.Invoke();
                    }
                }
                else
                {
                    GUI.enabled = false;
                    GUILayout.Button($"[ NEED ${cost} ]", GUILayout.Width(130), GUILayout.Height(26));
                    GUI.enabled = true;
                }
            }
            else
            {
                GUILayout.Label($"Effect: <b>{currentVal}</b> (Maxed)");
                GUI.enabled = false;
                GUILayout.Button("[ MAX LEVEL ]", GUILayout.Width(130), GUILayout.Height(26));
                GUI.enabled = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
}

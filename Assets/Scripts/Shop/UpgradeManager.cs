using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Inventory;
using RentIsDue.Player;

namespace RentIsDue.Shop
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public const int MAX_LEVEL = 5;

        private int GetCost(int baseCost, int level)
        {
            float eventMultiplier = RentIsDue.Gameplay.RandomEventManager.Instance != null ? RentIsDue.Gameplay.RandomEventManager.Instance.GetUpgradeCostMultiplier() : 1f;
            return Mathf.RoundToInt(baseCost * level * eventMultiplier);
        }

        public int GetBackpackUpgradeCost() => GetCost(50, backpackLevel);
        public int GetCarryWeightUpgradeCost() => GetCost(40, carryWeightLevel);
        public int GetStaminaUpgradeCost() => GetCost(60, staminaLevel);
        public int GetStaminaRegenUpgradeCost() => GetCost(60, staminaRegenLevel);
        public int GetMoveSpeedUpgradeCost() => GetCost(100, moveSpeedLevel);
        public int GetSearchSpeedUpgradeCost() => GetCost(80, searchSpeedLevel);
        public int GetSellPriceUpgradeCost() => GetCost(150, sellPriceLevel);
        public int GetRepairSpeedUpgradeCost() => GetCost(70, repairSpeedLevel);

        [Header("Upgrade Levels (1-5)")]
        public int backpackLevel = 1;
        public int carryWeightLevel = 1;
        public int staminaLevel = 1;
        public int staminaRegenLevel = 1;
        public int moveSpeedLevel = 1;
        public int searchSpeedLevel = 1;
        public int sellPriceLevel = 1;
        public int repairSpeedLevel = 1;

        // Data arrays based on UPGRADE_DATABASE.md
        public static readonly int[] BackpackSlots = { 8, 12, 16, 22, 30 };
        public static readonly int[] BackpackCosts = { 0, 80, 200, 450, 950 };

        public static readonly float[] CarryWeights = { 20f, 28f, 36f, 48f, 65f };
        public static readonly int[] CarryWeightCosts = { 0, 90, 220, 500, 1100 };

        public static readonly int[] StaminaValues = { 100, 125, 150, 180, 220 };
        public static readonly int[] StaminaCosts = { 0, 85, 230, 550, 1200 };

        public static readonly float[] StaminaRegenValues = { 4.0f, 7.0f, 11.0f, 16.0f, 22.0f };
        public static readonly int[] StaminaRegenCosts = { 0, 80, 220, 500, 1100 };

        public static readonly float[] MoveSpeedMultipliers = { 1.00f, 1.10f, 1.25f, 1.40f, 1.60f };
        public static readonly int[] MoveSpeedCosts = { 0, 90, 240, 550, 1200 };

        public static readonly float[] SearchSpeedMultipliers = { 1.00f, 1.20f, 1.50f, 1.85f, 2.25f };
        public static readonly int[] SearchSpeedCosts = { 0, 100, 260, 600, 1400 };

        public static readonly float[] SellPriceMultipliers = { 1.00f, 1.05f, 1.10f, 1.17f, 1.25f };
        public static readonly int[] SellPriceCosts = { 0, 150, 380, 850, 1800 };

        public static readonly float[] RepairSpeedMultipliers = { 1.00f, 1.35f, 1.85f, 2.50f, 3.50f };
        public static readonly int[] RepairSpeedCosts = { 0, 110, 270, 620, 1350 };

        // Modifiers
        public int GetMaxSlots() => BackpackSlots[Mathf.Clamp(backpackLevel - 1, 0, 4)];
        public float GetMaxWeight() => CarryWeights[Mathf.Clamp(carryWeightLevel - 1, 0, 4)];
        public int GetMaxStamina() => StaminaValues[Mathf.Clamp(staminaLevel - 1, 0, 4)];
        public float GetStaminaRegenRate() => StaminaRegenValues[Mathf.Clamp(staminaRegenLevel - 1, 0, 4)];
        public float GetMoveSpeedMultiplier() => MoveSpeedMultipliers[Mathf.Clamp(moveSpeedLevel - 1, 0, 4)];
        public float GetSearchSpeedMultiplier() => SearchSpeedMultipliers[Mathf.Clamp(searchSpeedLevel - 1, 0, 4)];
        public float GetSellPriceMultiplier() => SellPriceMultipliers[Mathf.Clamp(sellPriceLevel - 1, 0, 4)];
        public float GetRepairSpeedMultiplier() => RepairSpeedMultipliers[Mathf.Clamp(repairSpeedLevel - 1, 0, 4)];

        // Helper for Search duration reduction: Duration = Base / Multiplier
        public float SearchSpeedMultiplier => 1f / GetSearchSpeedMultiplier();

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
            ApplyAllUpgrades();
        }

        public void ApplyAllUpgrades()
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.maxSlots = GetMaxSlots();
                InventoryManager.Instance.maxWeight = GetMaxWeight();
            }

            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            if (player != null)
            {
                player.moveSpeed = 5.5f * GetMoveSpeedMultiplier();
            }
        }

        public bool TryUpgrade(ref int currentLevel, int[] costs, System.Action onUpgraded)
        {
            if (currentLevel >= MAX_LEVEL) return false;
            int nextCost = costs[currentLevel]; // index currentLevel is next cost

            if (EconomyManager.Instance != null && EconomyManager.Instance.currentMoney >= nextCost)
            {
                EconomyManager.Instance.currentMoney -= nextCost;
                currentLevel++;
                onUpgraded?.Invoke();
                ApplyAllUpgrades();

                if (RentIsDue.Core.PlaytestLogger.Instance != null)
                {
                    RentIsDue.Core.PlaytestLogger.Instance.RecordFirstUpgrade();
                }

                return true;
            }
            return false;
        }
    }
}

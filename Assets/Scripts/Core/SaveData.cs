using System;

namespace RentIsDue.Core
{
    [Serializable]
    public class SaveData
    {
        public int currentDay;
        public float currentMoney;
        public float currentDebt;
        public int backpackLevel = 1;
        public int carryWeightLevel = 1;
        public int staminaLevel = 1;
        public int staminaRegenLevel = 1;
        public int moveSpeedLevel = 1;
        public int searchSpeedLevel = 1;
        public int sellPriceLevel = 1;
        public System.Collections.Generic.List<SavedItem> inventory = new System.Collections.Generic.List<SavedItem>();
    }

    [Serializable]
    public class SavedItem
    {
        public string id;
        public float condition;
    }
}

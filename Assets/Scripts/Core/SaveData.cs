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
        public int repairSpeedLevel = 1;
        public bool isStorageUnlocked = false;
        public System.Collections.Generic.List<SavedItem> inventory = new System.Collections.Generic.List<SavedItem>();
        public System.Collections.Generic.List<SavedFloorItem> floorItems = new System.Collections.Generic.List<SavedFloorItem>();
        public System.Collections.Generic.List<string> currentCollectorSet = new System.Collections.Generic.List<string>();
    }

    [Serializable]
    public class SavedItem
    {
        public string id;
        public float condition;
    }

    [Serializable]
    public class SavedFloorItem
    {
        public string id;
        public float condition;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ, rotW;
    }
}

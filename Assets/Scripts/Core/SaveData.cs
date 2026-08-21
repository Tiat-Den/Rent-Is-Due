using System;

namespace RentIsDue.Core
{
    [Serializable]
    public class SaveData
    {
        public int currentDay;
        public float currentMoney;
        public int backpackLevel;
        public int movementLevel;
        public int searchSpeedLevel;
    }
}

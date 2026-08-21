using UnityEngine;
using RentIsDue.Economy;

namespace RentIsDue.Core
{
    public class TimeUI : MonoBehaviour
    {
        private void OnGUI()
        {
            if (TimeManager.Instance == null || DayManager.Instance == null) return;

            string timeString = TimeManager.Instance.GetTimeString();
            int day = DayManager.Instance.currentDay;
            int rent = DayManager.Instance.currentRent;

            GUI.Label(new Rect(10, 10, 300, 20), $"Day: {day}");
            GUI.Label(new Rect(10, 30, 300, 20), $"Time: {timeString}");
            GUI.Label(new Rect(10, 50, 300, 20), $"Rent Required: ${rent}");
            
            if (EconomyManager.Instance != null)
            {
                float currentMoney = EconomyManager.Instance.currentMoney;
                GUI.Label(new Rect(10, 70, 300, 20), $"Money: ${currentMoney}");
            }
        }
    }
}

using UnityEngine;
using RentIsDue.Economy;

namespace RentIsDue.Core
{
    public class TimeUI : MonoBehaviour
    {
        private void OnGUI()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;
            if (TimeManager.Instance == null || DayManager.Instance == null) return;

            string timeString = TimeManager.Instance.GetTimeString();
            int day = DayManager.Instance.currentDay;
            int rent = DayManager.Instance.currentRent;

            float startX = Screen.width / 2f - 75f;
            GUI.Label(new Rect(startX, 10, 150, 20), $"Day: {day}");
            GUI.Label(new Rect(startX, 30, 150, 20), $"Time: {timeString}");
            GUI.Label(new Rect(startX, 50, 150, 20), $"Rent Required: ${rent}");
        }
    }
}

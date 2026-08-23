using UnityEngine;
using RentIsDue.Economy;

namespace RentIsDue.Core
{
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        public int currentDay = 1;
        public int currentRent { get; private set; }

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
            CalculateRent();
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded += HandleDayEnded;
            }
        }

        private void OnDestroy()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayEnded -= HandleDayEnded;
            }
        }

        public void CalculateRent()
        {
            currentRent = Mathf.RoundToInt(100 * Mathf.Pow(1.25f, currentDay - 1));
        }

        private void HandleDayEnded()
        {
            if (EconomyManager.Instance.currentMoney >= currentRent)
            {
                int rentToPay = currentRent;
                EconomyManager.Instance.currentMoney -= rentToPay;
                int nextRent = Mathf.RoundToInt(100 * Mathf.Pow(1.25f, currentDay));

                if (DaySummaryUI.Instance != null)
                {
                    DaySummaryUI.Instance.ShowDayPassed(currentDay, rentToPay, EconomyManager.Instance.currentMoney, nextRent);
                }
                else
                {
                    ProceedToNextDay();
                }
            }
            else
            {
                Debug.Log("GAME OVER: Not enough money for rent.");
                if (DaySummaryUI.Instance != null)
                {
                    DaySummaryUI.Instance.ShowGameOver(currentDay, currentRent, EconomyManager.Instance.currentMoney);
                }
                else
                {
                    TimeManager.Instance.isTimeRunning = false;
                }
            }
        }

        public void ProceedToNextDay()
        {
            currentDay++;
            CalculateRent();
            Debug.Log($"Next Day: Day {currentDay}. Paid rent.");
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ResetToMorning();
            }
        }
    }
}

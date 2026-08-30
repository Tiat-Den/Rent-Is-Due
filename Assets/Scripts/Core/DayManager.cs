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
            int baseRent = Mathf.RoundToInt(100 * Mathf.Pow(1.25f, currentDay - 1));
            int debtAmount = 0;
            if (DebtManager.Instance != null)
            {
                debtAmount = Mathf.RoundToInt(DebtManager.Instance.currentDebt);
            }
            currentRent = baseRent + debtAmount;
        }

        private void HandleDayEnded()
        {
            if (EconomyManager.Instance == null)
            {
                Debug.LogError("[DayManager] EconomyManager.Instance is null — cannot process day end!");
                return;
            }

            if (EconomyManager.Instance.currentMoney >= currentRent)
            {
                int rentToPay = currentRent;
                EconomyManager.Instance.currentMoney -= rentToPay;
                int nextRent = Mathf.RoundToInt(100 * Mathf.Pow(1.25f, currentDay));

                // Kiểm tra điều kiện Victory
                bool hasStorage = RentIsDue.Shop.UpgradeManager.Instance != null && RentIsDue.Shop.UpgradeManager.Instance.isStorageUnlocked;
                if (EconomyManager.Instance.currentMoney >= 50000f && hasStorage)
                {
                    Debug.Log("VICTORY! Sống sót, mua nhà kho và dư $50,000!");
                    if (DaySummaryUI.Instance != null)
                    {
                        // TODO: Gọi UI Victory (Hiện tạm dùng ShowGameOver hoặc tạo màn mới)
                        // Giả sử có hàm ShowVictory
                        DaySummaryUI.Instance.ShowVictory(currentDay, rentToPay, EconomyManager.Instance.currentMoney);
                    }
                    return;
                }

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
                    DaySummaryUI.Instance.ShowGameOver(currentDay, EconomyManager.Instance.currentMoney, currentRent);
                }
                else
                {
                    if (TimeManager.Instance != null)
                        TimeManager.Instance.isTimeRunning = false;
                }
            }
        }

        public void ProceedToNextDay()
        {
            currentDay++;
            CalculateRent();
            
            if (PlaytestLogger.Instance != null)
            {
                PlaytestLogger.Instance.RecordDayReached(currentDay);
            }

            Debug.Log($"Next Day: Day {currentDay}. Paid rent.");
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ResetToMorning();
            }

            if (RentIsDue.Gameplay.RandomEventManager.Instance != null)
            {
                RentIsDue.Gameplay.RandomEventManager.Instance.RollDailyEvent();
            }
        }
    }
}

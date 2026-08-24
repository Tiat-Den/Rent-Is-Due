using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Audio;

namespace RentIsDue.Core
{
    public class DaySummaryUI : MonoBehaviour
    {
        private static DaySummaryUI _instance;
        public static DaySummaryUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<DaySummaryUI>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("DaySummaryUI");
                        _instance = go.AddComponent<DaySummaryUI>();
                    }
                }
                return _instance;
            }
        }

        public bool isShowingSummary { get; private set; } = false;
        private bool isGameOver = false;

        private int dayPassed;
        private int rentPaid;
        private float remainingMoney;
        private int nextDayRent;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        public void ShowDayPassed(int day, int rent, float remaining, int nextRent)
        {
            isShowingSummary = true;
            isGameOver = false;
            dayPassed = day;
            rentPaid = rent;
            remainingMoney = remaining;
            nextDayRent = nextRent;

            Time.timeScale = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDayPass();
        }

        public void ShowGameOver(int day, int rentNeeded, float currentMoney)
        {
            isShowingSummary = true;
            isGameOver = true;
            dayPassed = day;
            rentPaid = rentNeeded;
            remainingMoney = currentMoney;

            Time.timeScale = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        }

        private void OnGUI()
        {
            if (!isShowingSummary) return;

            float width = 420;
            float height = 340;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            string title = isGameOver ? "☠️ EVICTION NOTICE — GAME OVER" : $"🌙 DAY {dayPassed} SUMMARY";
            GUILayout.BeginArea(new Rect(x, y, width, height), title, GUI.skin.window);
            
            GUILayout.Space(20);

            if (!isGameOver)
            {
                GUILayout.Label($"<size=16><b>Day {dayPassed} Completed Successfully!</b></size>");
                GUILayout.Space(10);
                GUILayout.Label($"Rent Paid: <color=red>-${rentPaid}</color>");
                GUILayout.Label($"Remaining Savings: <color=green><b>${remainingMoney:F1}</b></color>");
                GUILayout.Label($"Tomorrow's Rent (Day {dayPassed + 1}): <color=yellow><b>${nextDayRent}</b></color>");
                
                GUILayout.Space(25);

                if (GUILayout.Button("🌅 START NEXT DAY", GUILayout.Height(40)))
                {
                    isShowingSummary = false;
                    Time.timeScale = 1f;
                    if (DayManager.Instance != null)
                    {
                        DayManager.Instance.ProceedToNextDay();
                    }
                    if (FloatingFeedbackUI.Instance != null)
                    {
                        FloatingFeedbackUI.Instance.ShowMessage($"Day {dayPassed + 1} Started! Rent: ${nextDayRent}", Color.green, 3f);
                    }
                }
            }
            else
            {
                float deficit = rentPaid - remainingMoney;
                bool canBorrow = DebtManager.Instance != null && DebtManager.Instance.CanTakeLoan(deficit);
                float loanWithInterest = deficit * 1.20f;

                GUILayout.Label("<size=16><b><color=red>YOU ARE SHORT ON RENT!</color></b></size>");
                GUILayout.Space(10);
                GUILayout.Label($"Rent Due: <color=red>${rentPaid}</color> | You have: <color=yellow>${remainingMoney:F1}</color>");
                GUILayout.Label($"Deficit: <color=red><b>-${deficit:F1}</b></color>");

                if (DebtManager.Instance != null && DebtManager.Instance.currentDebt > 0f)
                {
                    GUILayout.Label($"Current Outstanding Debt: <color=orange>${DebtManager.Instance.currentDebt:F1}</color>");
                }

                GUILayout.Space(15);

                // Nút Vay nợ khẩn cấp của chủ nhà (Lãi 20%)
                if (canBorrow)
                {
                    if (GUILayout.Button($"🤝 TAKE LANDLORD LOAN (+${deficit:F1} now ➔ Owe ${loanWithInterest:F1})", GUILayout.Height(40)))
                    {
                        DebtManager.Instance.TakeEmergencyLoan(deficit);
                        if (EconomyManager.Instance != null) EconomyManager.Instance.currentMoney = 0f;
                        isShowingSummary = false;
                        Time.timeScale = 1f;
                        if (DayManager.Instance != null) DayManager.Instance.ProceedToNextDay();
                        if (FloatingFeedbackUI.Instance != null)
                        {
                            FloatingFeedbackUI.Instance.ShowMessage($"Borrowed ${deficit:F1}! Survived to Day {dayPassed + 1}", Color.yellow, 3.5f);
                        }
                    }
                    GUILayout.Space(8);
                }

                if (GUILayout.Button("🔄 RESTART FROM DAY 1", GUILayout.Height(30)))
                {
                    isShowingSummary = false;
                    Time.timeScale = 1f;
                    if (EconomyManager.Instance != null) EconomyManager.Instance.currentMoney = 0f;
                    if (DebtManager.Instance != null) DebtManager.Instance.currentDebt = 0f;
                    if (DayManager.Instance != null)
                    {
                        DayManager.Instance.currentDay = 1;
                        DayManager.Instance.CalculateRent();
                        TimeManager.Instance.ResetToMorning();
                    }
                }

                GUILayout.Space(5);

                if (GUILayout.Button("📂 LOAD LAST SAVE", GUILayout.Height(30)))
                {
                    isShowingSummary = false;
                    Time.timeScale = 1f;
                    if (SaveManager.Instance != null) SaveManager.Instance.LoadGame();
                }
            }

            GUILayout.EndArea();
        }
    }
}

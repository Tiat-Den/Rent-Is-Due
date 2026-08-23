using UnityEngine;

namespace RentIsDue.Economy
{
    public class DebtManager : MonoBehaviour
    {
        public static DebtManager Instance { get; private set; }

        [Header("Debt & Loan Settings")]
        public float currentDebt = 0f;
        public float dailyInterestRate = 0.20f; // 20% lãi mỗi ngày
        public float maxLoanAllowed = 500f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool CanTakeLoan(float amountNeeded)
        {
            return (currentDebt + amountNeeded) <= maxLoanAllowed;
        }

        public void TakeEmergencyLoan(float amountShort)
        {
            float debtWithInterest = amountShort * (1f + dailyInterestRate);
            currentDebt += debtWithInterest;
            Debug.Log($"<color=orange>[DebtManager] Borrowed ${amountShort:F1}. New Debt with 20% interest: ${currentDebt:F1}</color>");
        }

        public void ApplyDailyInterest()
        {
            if (currentDebt > 0f)
            {
                float interest = currentDebt * dailyInterestRate;
                currentDebt += interest;
                Debug.Log($"[DebtManager] Daily 20% interest applied (+${interest:F1}). Total Debt: ${currentDebt:F1}");
            }
        }
    }
}

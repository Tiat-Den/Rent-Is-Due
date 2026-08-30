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
        private bool isVictory = false;

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
            isVictory = false;

            dayPassed = day;
            rentPaid = rent;
            remainingMoney = remaining;
            nextDayRent = nextRent;
            Time.timeScale = 0f;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            SaveManager.Instance?.SaveGame();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDayPass();
        }

        public void ShowGameOver(int day, float currentMoney, int missingRent)
        {
            isShowingSummary = true;
            isGameOver = true;
            isVictory = false;

            dayPassed = day;
            nextDayRent = missingRent;
            remainingMoney = currentMoney;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (PlaytestLogger.Instance != null) PlaytestLogger.Instance.RecordGameOver();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayGameOver();
        }

        public void ShowVictory(int day, int rent, float currentMoney)
        {
            isShowingSummary = true;
            isVictory = true;
            isGameOver = false;

            dayPassed = day;
            rentPaid = rent;
            remainingMoney = currentMoney;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySell();
        }

        private void OnGUI()
        {
            if (!isShowingSummary) return;

            float width = 400;
            float height = 300;
            float x = (Screen.width - width) / 2f;
            float y = (Screen.height - height) / 2f;

            if (isGameOver)
            {
                GUI.Window(0, new Rect(x, y, width, height), DrawGameOverWindow, "GAME OVER");
            }
            else if (isVictory)
            {
                GUI.Window(3, new Rect(x, y, width, height), DrawVictoryWindow, "CHIEN THANG!");
            }
            else
            {
                GUI.Window(0, new Rect(x, y, width, height), DrawSummaryWindow, "TONG KET NGAY");
            }
        }

        private void DrawSummaryWindow(int windowID)
        {
            GUILayout.Label($"<b>KET THUC NGAY {dayPassed}</b>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 24 });
            GUILayout.Space(20);

            GUILayout.Label($"- DA TRA TIEN NHA: ${rentPaid}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            GUILayout.Label($"- SO TIEN CON LAI: ${remainingMoney:F1}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
            
            GUILayout.Space(10);
            GUILayout.Label($"- TIEN NHA PHAI DONG NGAY MAI: ${nextDayRent}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            GUILayout.Space(30);
            if (GUILayout.Button("NGU / MUA NANG CAP", GUILayout.Height(50)))
            {
                isShowingSummary = false;
                if (RentIsDue.Shop.UpgradeUI.Instance != null)
                {
                    RentIsDue.Shop.UpgradeUI.Instance.ShowUI();
                }
                else
                {
                    DayManager.Instance?.ProceedToNextDay();
                }
            }
        }

        private void DrawGameOverWindow(int windowID)
        {
            GUILayout.Label($"<b>NGAY {dayPassed} - KET THUC</b>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 24 });
            GUILayout.Space(20);
            GUILayout.Label("Ban khong du tien dong nha va bi duoi ra ngoai.");
            GUILayout.Label($"Tien ban co: ${remainingMoney:F1}");
            
            GUILayout.Space(30);
            if (GUILayout.Button("QUAY VE MENU CHINH", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        private void DrawVictoryWindow(int windowID)
        {
            GUILayout.Label("Ban da thoat khoi canh no nan!");
            
            GUILayout.Space(30);
            if (GUILayout.Button("QUAY VE MENU CHINH", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }
}

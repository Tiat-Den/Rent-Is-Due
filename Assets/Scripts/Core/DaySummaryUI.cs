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
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySell();
        }

        public void ShowGameOver(int day, int missingRent, float currentMoney)
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
            if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
        }

        public void ShowVictory(int day, int rent, float currentMoney)
        {
            isShowingSummary = true;
            isGameOver = false;
            isVictory = true;
            dayPassed = day;
            rentPaid = rent;
            remainingMoney = currentMoney;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySell();
        }

        public void Hide()
        {
            isShowingSummary = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnGUI()
        {
            if (!isShowingSummary) return;

            float w = 400f;
            float h = 300f;
            Rect rect = new Rect(Screen.width / 2f - w / 2f, Screen.height / 2f - h / 2f, w, h);
            
            if (isVictory)
            {
                GUI.Window(9001, rect, DrawVictoryWindow, "🏆 WINNER WINNER 🏆");
            }
            else if (isGameOver)
            {
                GUI.Window(9001, rect, DrawGameOverWindow, "GAME OVER");
            }
            else
            {
                GUI.Window(9001, rect, DrawSummaryWindow, "TỔNG KẾT NGÀY");
            }
        }

        private void DrawSummaryWindow(int id)
        {
            GUI.DrawTexture(new Rect(0, 0, 400, 300), Texture2D.blackTexture);
            GUILayout.Space(20);
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            GUILayout.Label($"ĐÃ SỐNG SÓT QUA NGÀY " + dayPassed + "!", titleStyle);
            
            GUILayout.Space(20);
            GUILayout.Label($"- Đã thanh toán nợ: ${rentPaid}");
            GUILayout.Label($"- Tiền dư hiện tại: ${remainingMoney:F1}");
            GUILayout.Space(10);
            GUILayout.Label($"- TIỀN NHÀ PHẢI ĐÓNG NGÀY MAI: ${nextDayRent}", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });

            GUILayout.Space(30);
            if (GUILayout.Button("BẮT ĐẦU NGÀY MỚI", GUILayout.Height(50)))
            {
                DayManager.Instance?.ProceedToNextDay();
            }
        }

        private void DrawGameOverWindow(int id)
        {
            GUI.DrawTexture(new Rect(0, 0, 400, 300), Texture2D.blackTexture);
            GUILayout.Space(20);
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.red;
            GUILayout.Label("BẠN ĐÃ BỊ ĐUỔI RA KHỎI NHÀ!", titleStyle);
            
            GUILayout.Space(20);
            GUILayout.Label($"Ngày tồn tại: {dayPassed}");
            GUILayout.Label($"Tiền nhà yêu cầu: ${nextDayRent}");
            GUILayout.Label($"Tiền bạn có: ${remainingMoney:F1}");
            
            GUILayout.Space(30);
            if (GUILayout.Button("QUAY VỀ MENU CHÍNH", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }

        private void DrawVictoryWindow(int id)
        {
            GUI.DrawTexture(new Rect(0, 0, 400, 300), Texture2D.blackTexture);
            GUILayout.Space(20);
            
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = Color.yellow;
            GUILayout.Label("🏆 CHIẾN THẮNG 🏆", titleStyle);
            
            GUILayout.Space(20);
            GUILayout.Label($"Bạn đã trả tiền nhà Ngày {dayPassed},");
            GUILayout.Label($"Và tích lũy thành công ${remainingMoney:F1}!");
            GUILayout.Label("Bạn đã thoát khỏi cảnh nợ nần!");
            
            GUILayout.Space(30);
            if (GUILayout.Button("QUAY VỀ MENU CHÍNH", GUILayout.Height(50)))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            }
        }
    }
}

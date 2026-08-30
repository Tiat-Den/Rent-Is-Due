using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace RentIsDue.Core
{
    public class TutorialManager : MonoBehaviour
    {
        private bool hasDismissed = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            if (SceneManager.GetActiveScene().name == "SampleScene")
            {
                GameObject go = new GameObject("TutorialManager");
                go.AddComponent<TutorialManager>();
            }
        }

        private void Update()
        {
            if (!hasDismissed && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                hasDismissed = true;
            }
        }

        private void OnGUI()
        {
            if (hasDismissed || (DayManager.Instance != null && DayManager.Instance.currentDay != 1))
            {
                return;
            }

            GUI.skin.box.alignment = TextAnchor.UpperLeft;
            
            string tutorialText = "HƯỚNG DẪN TÂN THỦ\n\n" +
                                  "W, A, S, D: Di chuyển\n" +
                                  "Chuột: Xoay góc nhìn\n" +
                                  "Phím E: Tương tác / Nhặt đồ / Bán đồ\n\n" +
                                  "MỤC TIÊU: Tìm các món đồ, đem tới chỗ Tủ Đồ (Dealer) để bán.\nPhải kiếm đủ tiền đóng tiền nhà trước 22:00!\n\n" +
                                  "[Nhấn SPACE để đóng]";

            GUI.Box(new Rect(20, 20, 450, 200), tutorialText);
        }
    }
}

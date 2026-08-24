using System.Collections;
using UnityEngine;
using RentIsDue.Core;
using RentIsDue.Player;
using RentIsDue.Audio;

namespace RentIsDue.Gameplay
{
    /// <summary>
    /// Attach to the bed object. Player presses E to sleep:
    ///  - Fade screen to black
    ///  - Fast-forward time to next morning (calls TimeManager.ResetToMorning)
    ///  - Triggers DayManager day-end flow (pays rent, advances day)
    ///  - Fade back in, player wakes up refreshed
    /// </summary>
    public class BedInteractable : MonoBehaviour, IInteractable
    {
        [Header("Sleep Settings")]
        [Tooltip("Seconds for fade-out + fade-in animation")]
        public float fadeDuration = 1.2f;

        [Header("UI — assign a full-screen black Image (CanvasGroup or raw Image)")]
        public UnityEngine.UI.Image fadeOverlay; // optional: auto-found if null

        private bool _isSleeping = false;

        private void Start()
        {
            // Try auto-find fade overlay in scene if not assigned
            if (fadeOverlay == null)
            {
                var go = GameObject.Find("Sleep_FadeOverlay");
                if (go != null) fadeOverlay = go.GetComponent<UnityEngine.UI.Image>();
            }
        }

        public bool CanInteract(PlayerInteractor player)
        {
            return !_isSleeping;
        }

        public string GetInteractionText()
        {
            if (_isSleeping) return "Đang ngủ...";

            if (TimeManager.Instance != null)
            {
                int hour = Mathf.FloorToInt(TimeManager.Instance.currentTimeMinutes / 60f);
                return hour < 20
                    ? $"Ngủ sớm (Hiện tại {hour:D2}:00 — sẽ bỏ qua đến sáng)"
                    : "Đi Ngủ (Kết thúc ngày)";
            }
            return "Đi Ngủ";
        }

        public void Interact(PlayerInteractor player)
        {
            if (!_isSleeping)
                StartCoroutine(SleepRoutine());
        }

        private IEnumerator SleepRoutine()
        {
            _isSleeping = true;

            // --- Fade OUT ---
            yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

            // --- Trigger day-end logic (pay rent, advance day) ---
            if (TimeManager.Instance != null)
            {
                // Fire the OnDayEnded event — DayManager handles rent & day advance
                TimeManager.Instance.TriggerDayEnd();
            }

            // Small pause to let day transition complete
            yield return new WaitForSeconds(0.5f);

            // Reset time to morning
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.ResetToMorning();
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayDayPass();

            // --- Fade IN ---
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

            _isSleeping = false;
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (fadeOverlay == null) yield break;

            float elapsed = 0f;
            Color c = fadeOverlay.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(from, to, elapsed / duration);
                fadeOverlay.color = new Color(c.r, c.g, c.b, alpha);
                yield return null;
            }
            fadeOverlay.color = new Color(c.r, c.g, c.b, to);
        }

        private void OnGUI()
        {
            if (!_isSleeping) return;
            GUI.color = new Color(1f, 1f, 0.7f, 1f);
            GUI.Label(new Rect(Screen.width / 2f - 120, Screen.height / 2f - 20, 240, 40),
                "💤 Đang ngủ... Sáng mai sẽ đến.", GUI.skin.box);
            GUI.color = Color.white;
        }
    }
}

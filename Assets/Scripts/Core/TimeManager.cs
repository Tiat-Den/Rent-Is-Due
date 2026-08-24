using UnityEngine;
using System;

namespace RentIsDue.Core
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        public event Action OnDayEnded;

        [Tooltip("How many real seconds equal one in-game minute")]
        public float realSecondsPerInGameMinute = 0.5f;

        // 08:00 = 8 * 60 = 480
        // 22:00 = 22 * 60 = 1320
        public int currentTimeMinutes = 480; 
        private float timer = 0f;
        public bool isTimeRunning = true;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (!isTimeRunning) return;

            timer += Time.deltaTime;
            if (timer >= realSecondsPerInGameMinute)
            {
                timer -= realSecondsPerInGameMinute;
                currentTimeMinutes++;

                if (currentTimeMinutes >= 1320) // 22:00
                {
                    EndDay();
                }
            }
        }

        private void EndDay()
        {
            isTimeRunning = false;
            OnDayEnded?.Invoke();
        }

        /// <summary>
        /// Manually trigger the day-end event (e.g. player chooses to sleep early).
        /// </summary>
        public void TriggerDayEnd()
        {
            if (isTimeRunning)
            {
                isTimeRunning = false;
            }
            OnDayEnded?.Invoke();
        }

        public void ResetToMorning()
        {
            currentTimeMinutes = 480;
            timer = 0f;
            isTimeRunning = true;
        }

        public string GetTimeString()
        {
            int hours = currentTimeMinutes / 60;
            int minutes = currentTimeMinutes % 60;
            return $"{hours:00}:{minutes:00}";
        }
    }
}

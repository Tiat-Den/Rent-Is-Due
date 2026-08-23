using System;
using System.IO;
using UnityEngine;

namespace RentIsDue.Core
{
    [Serializable]
    public class PlaytestMetrics
    {
        public string playtestStartTime;
        public float totalPlayTimeSeconds;
        public float timeToFirstItemSeconds = -1f;
        public float timeToFirstSaleSeconds = -1f;
        public float timeToFirstUpgradeSeconds = -1f;
        public int totalItemsFound;
        public int totalItemsSold;
        public float totalMoneyEarned;
        public int maxDayReached = 1;
        public int timesGameOver;
        public int timesLoanTaken;
    }

    public class PlaytestLogger : MonoBehaviour
    {
        public static PlaytestLogger Instance { get; private set; }

        public PlaytestMetrics metrics = new PlaytestMetrics();
        private float sessionStartTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sessionStartTime = Time.time;
            metrics.playtestStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void RecordFirstItem()
        {
            if (metrics.timeToFirstItemSeconds < 0)
            {
                metrics.timeToFirstItemSeconds = Time.time - sessionStartTime;
                SaveMetrics();
            }
            metrics.totalItemsFound++;
        }

        public void RecordFirstSale(float amount)
        {
            if (metrics.timeToFirstSaleSeconds < 0)
            {
                metrics.timeToFirstSaleSeconds = Time.time - sessionStartTime;
            }
            metrics.totalItemsSold++;
            metrics.totalMoneyEarned += amount;
            SaveMetrics();
        }

        public void RecordFirstUpgrade()
        {
            if (metrics.timeToFirstUpgradeSeconds < 0)
            {
                metrics.timeToFirstUpgradeSeconds = Time.time - sessionStartTime;
                SaveMetrics();
            }
        }

        public void RecordDayReached(int day)
        {
            if (day > metrics.maxDayReached)
            {
                metrics.maxDayReached = day;
                SaveMetrics();
            }
        }

        public void RecordLoanTaken()
        {
            metrics.timesLoanTaken++;
            SaveMetrics();
        }

        public void RecordGameOver()
        {
            metrics.timesGameOver++;
            SaveMetrics();
        }

        private void OnApplicationQuit()
        {
            metrics.totalPlayTimeSeconds = Time.time - sessionStartTime;
            SaveMetrics();
        }

        public void SaveMetrics()
        {
            metrics.totalPlayTimeSeconds = Time.time - sessionStartTime;
            string json = JsonUtility.ToJson(metrics, true);
            string path = Path.Combine(Application.persistentDataPath, "playtest_metrics.json");
            File.WriteAllText(path, json);
        }
    }
}

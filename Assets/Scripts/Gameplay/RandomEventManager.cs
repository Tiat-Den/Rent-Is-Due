using UnityEngine;
using RentIsDue.Economy;
using RentIsDue.Core;

namespace RentIsDue.Gameplay
{
    public enum DailyEventType
    {
        None,
        MarketBoom,   // +30% giá bán
        MarketCrash,  // -30% giá bán
        BadWeather,   // Trừ $20 tiền lò sưởi / điện
        Thief         // Bị trộm mất 50% tiền
    }

    public class RandomEventManager : MonoBehaviour
    {
        public static RandomEventManager Instance { get; private set; }

        public DailyEventType currentEvent = DailyEventType.None;
        public string eventName = "";
        public string eventDescription = "";
        public Color eventColor = Color.white;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RollDailyEvent()
        {
            // Tỷ lệ 30% không có sự kiện gì
            float roll = Random.value;
            if (roll < 0.3f)
            {
                SetEvent(DailyEventType.None);
                return;
            }

            // 70% có sự kiện, chọn ngẫu nhiên 1 trong 4 sự kiện còn lại
            int eventIndex = Random.Range(1, 5); // 1 to 4
            SetEvent((DailyEventType)eventIndex);
        }

        private void SetEvent(DailyEventType type)
        {
            currentEvent = type;
            switch (type)
            {
                case DailyEventType.None:
                    eventName = "Một Ngày Bình Yên";
                    eventDescription = "Không có sự kiện đặc biệt nào xảy ra hôm nay.";
                    eventColor = Color.white;
                    break;

                case DailyEventType.MarketBoom:
                    eventName = "📈 THỊ TRƯỜNG SÔI ĐỘNG";
                    eventDescription = "Giá thu mua phế liệu tăng 30% trong hôm nay!";
                    eventColor = Color.green;
                    break;

                case DailyEventType.MarketCrash:
                    eventName = "📉 THỊ TRƯỜNG SUY THOÁI";
                    eventDescription = "Giá thu mua phế liệu giảm 30% trong hôm nay!";
                    eventColor = new Color(1f, 0.4f, 0.4f); // Red
                    break;

                case DailyEventType.BadWeather:
                    eventName = "⛈️ THỜI TIẾT XẤU";
                    eventDescription = "Trời lạnh và bão, bạn phải tốn thêm $20 tiền lò sưởi.";
                    eventColor = Color.blue;
                    if (EconomyManager.Instance != null) EconomyManager.Instance.currentMoney -= 20f;
                    break;

                case DailyEventType.Thief:
                    bool hasSafe = false;
                    if (RentIsDue.Inventory.InventoryManager.Instance != null)
                    {
                        foreach (var item in RentIsDue.Inventory.InventoryManager.Instance.items)
                        {
                            if (item.data != null && item.data.id == "item_safe")
                            {
                                hasSafe = true;
                                break;
                            }
                        }
                    }

                    if (hasSafe)
                    {
                        eventName = "🛡️ KÉT SẮT CỨU CÁNH";
                        eventDescription = "Có trộm đột nhập đêm qua nhưng bạn cất tiền trong Két Sắt nên không mất mát gì!";
                        eventColor = Color.yellow;
                    }
                    else
                    {
                        eventName = "🥷 KẺ TRỘM GHÉ THĂM";
                        eventDescription = "Cửa nẻo lỏng lẻo, bạn bị trộm cuỗm mất 50% tiền tiết kiệm!";
                        eventColor = Color.red;
                        if (EconomyManager.Instance != null)
                        {
                            float lost = EconomyManager.Instance.currentMoney * 0.5f;
                            EconomyManager.Instance.currentMoney -= lost;
                        }
                    }
                    break;
            }
            
            // Hiển thị thông báo sự kiện buổi sáng
            if (MorningEventUI.Instance != null)
            {
                MorningEventUI.Instance.ShowEvent(eventName, eventDescription, eventColor);
            }
        }

        public float GetSellMultiplier()
        {
            if (currentEvent == DailyEventType.MarketBoom) return 1.3f;
            if (currentEvent == DailyEventType.MarketCrash) return 0.7f;
            return 1.0f;
        }

        public float GetUpgradeCostMultiplier()
        {
            if (currentEvent == DailyEventType.LuckyDay) return 0.5f;
            return 1.0f;
        }
    }
}

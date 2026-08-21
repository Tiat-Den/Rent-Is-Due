using UnityEngine;

namespace RentIsDue.Shop
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public int backpackLevel = 1;
        public int movementLevel = 1;
        public int searchSpeedLevel = 1;

        public float SearchSpeedMultiplier
        {
            get
            {
                // 1.0f at lv1, 0.8f at lv2, 0.6f at lv3, etc.
                return Mathf.Max(0.1f, 1.0f - ((searchSpeedLevel - 1) * 0.2f));
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
    }
}

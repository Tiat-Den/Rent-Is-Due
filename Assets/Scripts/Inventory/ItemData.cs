using UnityEngine;

namespace RentIsDue.Inventory
{
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ItemCategory
    {
        Trash,
        Paper,
        Container,
        Clothing,
        Toy,
        Kitchen,
        Home,
        Electronics,
        Gaming,
        Antique,
        Collectible,
        Art,
        Jewelry,
        Other
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "RentIsDue/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Basic Info")]
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemRarity rarity;
        public ItemCategory category;

        [Header("Economy & Physics")]
        public float baseValue;
        public float weight;
        public float searchTimeSec = 2f;
        public int maxStack = 1;
        public float collectorMultiplier = 1f;

        [Header("Visuals")]
        public Sprite icon;
        public GameObject prefab;
    }
}

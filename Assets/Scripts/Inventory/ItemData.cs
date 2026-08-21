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
        Junk,
        Electronics,
        Material,
        Valuable
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "RentIsDue/Item Data")]
    public class ItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemRarity rarity;
        public ItemCategory category;
        public float baseValue;
        public float weight;
        public Sprite icon;
        public GameObject prefab;
    }
}

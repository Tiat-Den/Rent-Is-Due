using System.Collections.Generic;
using UnityEngine;
using RentIsDue.Inventory;

namespace RentIsDue.Loot
{
    [System.Serializable]
    public class LootDrop
    {
        public ItemData item;
        public float weight;
    }

    [CreateAssetMenu(fileName = "NewLootTable", menuName = "RentIsDue/Loot Table")]
    public class LootTable : ScriptableObject
    {
        public List<LootDrop> drops = new List<LootDrop>();
        public float emptyWeight = 50f; // Weight to drop nothing

        public ItemData RollLoot()
        {
            float totalWeight = emptyWeight;
            foreach (var drop in drops)
            {
                totalWeight += drop.weight;
            }

            float roll = Random.Range(0, totalWeight);
            float currentWeight = emptyWeight;

            if (roll < currentWeight)
            {
                return null;
            }

            foreach (var drop in drops)
            {
                currentWeight += drop.weight;
                if (roll < currentWeight)
                {
                    return drop.item;
                }
            }

            return null;
        }
    }
}

namespace RentIsDue.Inventory
{
    /// <summary>
    /// Runtime wrapper around an ItemData ScriptableObject.
    /// Tracks per-instance mutable state like condition (0..1).
    /// ItemData itself stays immutable (shared ScriptableObject asset).
    /// </summary>
    [System.Serializable]
    public class ItemInstance
    {
        public ItemData data;

        /// <summary>
        /// 1.0 = perfect, 0.5 = damaged (-50% sell price), 0.0 = broken (unsellable without repair).
        /// </summary>
        public float condition = 1f;

        public bool IsDamaged => condition < 0.8f;

        /// <summary>Effective sell value considering condition.</summary>
        public float EffectiveValue => data != null ? data.baseValue * condition : 0f;

        /// <summary>Cost to fully repair this item (20% of base value minimum $1).</summary>
        public float RepairCost => data != null ? Mathf.Max(1f, data.baseValue * (1f - condition) * 0.4f) : 0f;

        public ItemInstance(ItemData itemData, float startCondition = 1f)
        {
            data = itemData;
            condition = Mathf.Clamp01(startCondition);
        }
    }
}

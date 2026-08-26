using UnityEngine;
using UnityEditor;
using RentIsDue.Inventory;

[InitializeOnLoad]
public static class ToolGenerator
{
    static ToolGenerator()
    {
        EditorApplication.delayCall += RunOnce;
    }

    static void RunOnce()
    {
        if (!System.IO.File.Exists("Assets/Resources/Items/item_screwdriver.asset"))
        {
            CreateTool("item_screwdriver", "Tuốc nơ vít", "Tăng tốc độ sửa chữa (x1.5)", 50f, 1f, ItemRarity.Epic);
            CreateTool("item_crowbar", "Xà beng", "Tăng tốc độ lục lọi (x1.5)", 75f, 2f, ItemRarity.Epic);
            CreateTool("item_safe", "Két sắt mini", "Chống mất tiền khi bị trộm", 150f, 5f, ItemRarity.Legendary);
            AssetDatabase.SaveAssets();
            Debug.Log("Generated Tools!");
        }
    }

    private static void CreateTool(string id, string name, string desc, float val, float w, ItemRarity r)
    {
        ItemData asset = ScriptableObject.CreateInstance<ItemData>();
        asset.id = id;
        asset.displayName = name;
        asset.description = desc;
        asset.baseValue = val;
        asset.weight = w;
        asset.rarity = r;
        AssetDatabase.CreateAsset(asset, "Assets/Resources/Items/" + id + ".asset");
    }
}

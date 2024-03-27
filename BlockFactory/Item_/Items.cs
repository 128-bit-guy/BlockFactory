using System.Diagnostics.CodeAnalysis;
using BlockFactory.Block_;
using BlockFactory.Registry_;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BlockFactory.Item_;

[SuppressMessage("Usage", "CA2211")]
public static class Items
{
    public static Registry<Item> Registry;
    public static Dictionary<Block, BlockItem> BlockItems;
    public static FertilizerItem Fertilizer = null!;
    public static PickaxeItem Pickaxe = null!;
    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<Item>("Item");
        BlockItems = new Dictionary<Block, BlockItem>();
        Blocks.Registry.ForEachEntry(RegisterItemForBlock);
        Fertilizer = Registry.Register("fertilizer", new FertilizerItem());
        Pickaxe = Registry.Register("pickaxe", new PickaxeItem());
    }

    private static void RegisterItemForBlock(string name, Block block)
    {
        var id = Blocks.Registry.GetForcedId(name);
        BlockItems.Add(block,
            id == null
                ? Registry.Register(name, new BlockItem(block))
                : Registry.RegisterForced(name, id.Value, new BlockItem(block)));
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}
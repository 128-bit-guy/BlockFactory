using BlockFactory.Block_;
using BlockFactory.Item_;
using BlockFactory.Registry_;

namespace BlockFactory.Init;

public class Items
{
    public static Registry<Item> Registry { get; private set; } = null!;
    public static AttachmentRegistry<Block, BlockItem> BlockItems { get; private set; } = null!;
    public static void Init()
    {
        Registry = SyncedRegistries.NewSyncedRegistry<Item>(new RegistryName("Items"));
        BlockItems = new AttachmentRegistry<Block, BlockItem>(Blocks.Registry);
        foreach (var block in Blocks.Registry.GetRegisteredEntries())
        {
            BlockItems.Register(block, Registry.Register(Blocks.Registry.GetName(block), 
                new BlockItem(block)));
        }
        Registry.Lock();
    }
}
using BlockFactory.Biome_;
using BlockFactory.Block_;
using BlockFactory.Item_;
using BlockFactory.Network.Packet_;

namespace BlockFactory;

public static class BfContent
{
    public static void Init()
    {
        Packets.Init();
        Packets.Lock();
        Blocks.Init();
        Items.Init();
        Biomes.Init();
        Blocks.Lock();
        Items.Lock();
        Biomes.Lock();
    }
}
using BlockFactory.Biome_;
using BlockFactory.Block_;
using BlockFactory.Network.Packet_;

namespace BlockFactory;

public static class BfContent
{
    public static void Init()
    {
        Packets.Init();
        Packets.Lock();
        Blocks.Init();
        Biomes.Init();
        Blocks.Lock();
        Biomes.Lock();
    }
}
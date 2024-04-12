using BlockFactory.Content.Biome_;
using BlockFactory.Content.Block_;
using BlockFactory.Content.Gui.Control;
using BlockFactory.Content.Item_;
using BlockFactory.Network.Packet_;

namespace BlockFactory.Content;

public static class BfContent
{
    public static void Init()
    {
        Packets.Init();
        Packets.Lock();
        Blocks.Init();
        Items.Init();
        Biomes.Init();
        SynchronizedControls.Init();
        Blocks.Lock();
        Items.Lock();
        Biomes.Lock();
        SynchronizedControls.Lock();
    }
}
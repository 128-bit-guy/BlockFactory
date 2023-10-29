using BlockFactory.Biome_;
using BlockFactory.Block_;

namespace BlockFactory;

public static class BfContent
{
    public static void Init()
    {
        Blocks.Init();
        Biomes.Init();
        Blocks.Lock();
        Biomes.Lock();
    }
}
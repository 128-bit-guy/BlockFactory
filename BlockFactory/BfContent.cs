using BlockFactory.Block_;

namespace BlockFactory;

public static class BfContent
{
    public static void Init()
    {
        Blocks.Init();
        Blocks.Lock();
    }
}
using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BlockFactory.Block_;

[SuppressMessage("Usage", "CA2211")]
public static class Blocks
{
    public static Registry<Block> Registry;
    public static AirBlock Air;
    public static SimpleBlock Stone;
    public static SimpleBlock Bricks;
    public static SimpleBlock Dirt;
    public static GrassBlock Grass;
    
    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<Block>("Block");
        Air = Registry.RegisterForced("Air", 0, new AirBlock());
        Stone = Registry.RegisterForced("Stone", 1, new SimpleBlock(0));
        Bricks = Registry.Register("Bricks", new SimpleBlock(16));
        Dirt = Registry.Register("Dirt", new SimpleBlock(2));
        Grass = Registry.Register("Grass", new GrassBlock());
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}
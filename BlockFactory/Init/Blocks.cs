using BlockFactory.Block_;
using BlockFactory.Registry_;

namespace BlockFactory.Init;

public class Blocks
{
    public static Registry<Block> Registry { get; private set; } = null!;
    public static AirBlock Air { get; private set; } = null!;
    public static DirtBlock Dirt { get; private set; } = null!;
    public static StoneBlock Stone { get; private set; } = null!;
    public static GrassBlock Grass { get; private set; } = null!;
    public static LogBlock Log { get; private set; } = null!;
    public static LeavesBlock Leaves { get; private set; } = null!;

    public static void Init()
    {
        Registry = SyncedRegistries.NewSyncedRegistry<Block>(new RegistryName("Blocks"));
        Air = Registry.Register(new RegistryName("Air"), new AirBlock());
        Dirt = Registry.Register(new RegistryName("Dirt"), new DirtBlock());
        Stone = Registry.Register(new RegistryName("Stone"), new StoneBlock());
        Grass = Registry.Register(new RegistryName("Grass"), new GrassBlock());
        Log = Registry.Register(new RegistryName("Log"), new LogBlock());
        Leaves = Registry.Register(new RegistryName("Leaves"), new LeavesBlock());
        Registry.Lock();
    }
}
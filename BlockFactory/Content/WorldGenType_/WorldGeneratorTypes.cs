using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;
using BlockFactory.World_.Gen;

namespace BlockFactory.Content.WorldGenType_;

[SuppressMessage("Usage", "CA2211")]
public class WorldGeneratorTypes
{
    public static Registry<WorldGeneratorType> Registry;
    public static WorldGeneratorType Terrain;
    public static WorldGeneratorType Flat;
    public static WorldGeneratorType Maze;

    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<WorldGeneratorType>("WorldGeneratorType");
        Terrain = Registry.RegisterForced("Terrain", 0, WorldGeneratorType.Create<WorldGenerator>());
        Flat = Registry.RegisterForced("Flat", 1, WorldGeneratorType.Create<FlatWorldGenerator>());
        Maze = Registry.Register("Maze", WorldGeneratorType.Create<MazeWorldGenerator>());
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}
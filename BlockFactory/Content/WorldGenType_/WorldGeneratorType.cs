using BlockFactory.Registry_;
using BlockFactory.Serialization;
using BlockFactory.World_.Gen;

namespace BlockFactory.Content.WorldGenType_;

public class WorldGeneratorType : IRegistryEntry
{
    public int Id { get; set; }
    public readonly string Name;
    public readonly Func<long, IWorldGenerator> WorldGeneratorCreator;

    public WorldGeneratorType(string name, Func<long, IWorldGenerator> worldGeneratorCreator)
    {
        Name = name;
        WorldGeneratorCreator = worldGeneratorCreator;
    }

    public static WorldGeneratorType Create<T>() where T : IWorldGenerator
    {
        var creator = typeof(T).GetConstructor(new[] { typeof(long) })!
            .CreateDelegate<Func<long, IWorldGenerator>>();
        var name = creator(-1).GetName();
        return new WorldGeneratorType(name, creator);
    }
}
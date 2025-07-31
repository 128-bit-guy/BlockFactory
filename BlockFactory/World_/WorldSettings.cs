using BlockFactory.Content.WorldGenType_;
using BlockFactory.Serialization;

namespace BlockFactory.World_;

public class WorldSettings : ITagSerializable
{
    public WorldGeneratorType GeneratorType;
    public long Seed;

    public WorldSettings(long seed, WorldGeneratorType generatorType)
    {
        Seed = seed;
        GeneratorType = generatorType;
    }

    public WorldSettings()
    {
        GeneratorType = WorldGeneratorTypes.Terrain;
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetValue("seed", Seed);
        res.SetValue("world_generator_type", GeneratorType.Id);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        Seed = tag.GetValue<long>("seed");
        if (tag.Keys.Contains("world_generator_type"))
        {
            GeneratorType = WorldGeneratorTypes.Registry[tag.GetValue<int>("world_generator_type")]!;
        }
        else
        {
            GeneratorType = tag.GetValue<bool>("flat")? WorldGeneratorTypes.Flat : WorldGeneratorTypes.Terrain;
        }
    }
}
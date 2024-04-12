using BlockFactory.Serialization;

namespace BlockFactory.World_;

public class WorldSettings : ITagSerializable
{
    public bool Flat;
    public long Seed;

    public WorldSettings(long seed, bool flat)
    {
        Seed = seed;
        Flat = flat;
    }

    public WorldSettings()
    {
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetValue("seed", Seed);
        res.SetValue("flat", Flat);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        Seed = tag.GetValue<long>("seed");
        Flat = tag.GetValue<bool>("flat");
    }
}
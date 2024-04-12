using BlockFactory.Serialization;

namespace BlockFactory.World_;

public class WorldData : ITagSerializable
{
    public WorldSettings WorldSettings;

    public WorldData(WorldSettings worldSettings)
    {
        WorldSettings = worldSettings;
    }

    public WorldData() : this(new WorldSettings())
    {
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.Set("world_settings", WorldSettings.SerializeToTag(reason));
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        WorldSettings.DeserializeFromTag(tag.Get<DictionaryTag>("world_settings"), reason);
    }
}
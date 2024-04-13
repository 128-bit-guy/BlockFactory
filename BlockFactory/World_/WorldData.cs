using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class WorldData : ITagSerializable
{
    public WorldSettings WorldSettings;
    public Vector3D<int>? SpawnPoint;

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
        if (SpawnPoint.HasValue)
        {
            res.SetVector3D("spawn_point", SpawnPoint.Value);
        }
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        WorldSettings.DeserializeFromTag(tag.Get<DictionaryTag>("world_settings"), reason);
        if (tag.Keys.Contains("spawn_point"))
        {
            SpawnPoint = tag.GetVector3D<int>("spawn_point");
        }
        else
        {
            SpawnPoint = null;
        }
    }
}
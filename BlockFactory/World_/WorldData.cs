using BlockFactory.Serialization.Automatic.Serializable;

namespace BlockFactory.World_;

public class WorldData : AutoSerializable
{
    public long LastId = 0;
    public long Time = 0;
    public int Seed = 0;
}
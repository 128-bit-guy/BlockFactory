using BlockFactory.Entity_.Player;
using BlockFactory.Serialization.Automatic;
using BlockFactory.Serialization.Automatic.Serializable;

namespace BlockFactory.Game;

public class PlayerInfo : AutoSerializable
{
    public Credentials Credentials;
    [NotSerialized]
    public PlayerEntity Player;

    public PlayerInfo(Credentials credentials)
    {
        Credentials = credentials;
    }
}
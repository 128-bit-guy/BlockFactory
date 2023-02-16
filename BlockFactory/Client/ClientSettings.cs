using BlockFactory.Game;
using BlockFactory.Serialization.Automatic.Serializable;
using BlockFactory.Side_;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public class ClientSettings : AutoSerializable
{
    public readonly Credentials Credentials = new();
}
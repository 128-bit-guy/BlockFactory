using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Side_;

namespace BlockFactory.Client.Game;

[ExclusiveTo(Side.Client)]
public class SingleplayerNetworkHandler : INetworkHandler
{
    public IEnumerable<NetworkConnection> GetAllConnections()
    {
        throw new InvalidOperationException("Can not get all connections in singleplayer");
    }

    public NetworkConnection GetPlayerConnection(PlayerEntity player)
    {
        throw new InvalidOperationException("Can not get player connection in singleplayer");
    }

    public NetworkConnection GetServerConnection()
    {
        throw new InvalidOperationException("Can not get server connection in singleplayer");
    }
}
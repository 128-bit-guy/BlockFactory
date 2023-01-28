using BlockFactory.Entity_.Player;
using BlockFactory.Network;
using BlockFactory.Entity_;

namespace BlockFactory.Game
{
    public interface INetworkHandler
    {
        IEnumerable<NetworkConnection> GetAllConnections();

        NetworkConnection GetServerConnection();

        NetworkConnection GetPlayerConnection(PlayerEntity player);
    }
}

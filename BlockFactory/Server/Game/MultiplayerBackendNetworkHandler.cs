using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Server.Entity_;

namespace BlockFactory.Server.Game
{
    public class MultiplayerBackendNetworkHandler : INetworkHandler
    {
        private readonly BlockFactoryServer _server;

        public MultiplayerBackendNetworkHandler(BlockFactoryServer server)
        {
            _server = server;
        }

        public IEnumerable<NetworkConnection> GetAllConnections()
        {
            return _server.Connections;
        }

        public NetworkConnection GetPlayerConnection(PlayerEntity player)
        {
            return ((ServerPlayerEntity)player).Connection;
        }

        public NetworkConnection GetServerConnection()
        {
            throw new InvalidOperationException("Can not get server connection on back end");
        }
    }
}

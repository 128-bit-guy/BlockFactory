using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;

namespace BlockFactory.Client.Game
{
    public class MultiplayerFrontendNetworkHandler : INetworkHandler
    {
        private readonly BlockFactoryClient _client;

        public MultiplayerFrontendNetworkHandler(BlockFactoryClient client)
        {
            _client = client;
        }

        public IEnumerable<NetworkConnection> GetAllConnections()
        {
            throw new InvalidOperationException("Can not get all connections on front end");
        }

        public NetworkConnection GetPlayerConnection(PlayerEntity player)
        {
            throw new InvalidOperationException("Can not get player connection on front end");
        }

        public NetworkConnection GetServerConnection()
        {
            return _client.ServerConnection!;
        }
    }
}

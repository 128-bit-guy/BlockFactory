using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;

namespace BlockFactory.Client.Game
{
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
}

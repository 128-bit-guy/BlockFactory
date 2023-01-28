using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Network;

namespace BlockFactory.Server.Entity_
{
    public class ServerPlayerEntity : PlayerEntity
    {
        public NetworkConnection Connection;

        public ServerPlayerEntity(NetworkConnection connection)
        {
            Connection = connection;
        }
    }
}

using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Network;
using BlockFactory.Side_;

namespace BlockFactory.Server.Entity_
{
    [ExclusiveTo(Side.Server)]
    public class ServerPlayerEntity : PlayerEntity
    {
        public NetworkConnection Connection;

        public ServerPlayerEntity(NetworkConnection connection)
        {
            Connection = connection;
        }
    }
}

using BlockFactory.Entity_.Player;
using BlockFactory.Network;
using BlockFactory.Side_;

namespace BlockFactory.Server.Entity_;

[ExclusiveTo(Side.Server)]
public class ServerPlayerEntity : PlayerEntity
{
    public NetworkConnection Connection;

    public ServerPlayerEntity(NetworkConnection connection)
    {
        Connection = connection;
    }

    private void ProcessPackets()
    {
        var cnt = Connection.ReceiveQueue.Count;
        for (var i = 0; i < cnt; ++i)
        {
            if (Connection.ReceiveQueue.TryDequeue(out var packet))
            {
                packet.Process(Connection);
            }
            else
            {
                break;
            }
        }
    }

    protected override void TickInternal()
    {
        ProcessPackets();
        base.TickInternal();
    }
}
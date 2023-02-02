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
        try
        {
            Connection.ProcessInGamePackets();
        }
        catch (Exception ex)
        {
            Connection.SetErrored(ex);
        }
    }

    protected override void TickInternal()
    {
        ProcessPackets();
        base.TickInternal();
    }
}
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Serialization.Automatic;
using BlockFactory.Side_;

namespace BlockFactory.Server.Entity_;

[ExclusiveTo(Side.Server)]
public class ServerPlayerEntity : PlayerEntity
{
    [NotSerialized]
    public NetworkConnection Connection = null!;

    public ServerPlayerEntity(PlayerInfo info) : base(info)
    {
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
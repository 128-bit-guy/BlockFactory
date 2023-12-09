using BlockFactory.Base;
using BlockFactory.Entity_;

namespace BlockFactory.Network;

[ExclusiveTo(Side.Client)]
public class SinglePlayerNetworkHandler : INetworkHandler
{
    public void Update()
    {
        
    }

    public bool ShouldStop()
    {
        return false;
    }

    public void SendPacket<T>(PlayerEntity? player, T packet) where T : class, IPacket
    {
        packet.Handle(player);
    }

    public void Dispose()
    {
        
    }
}
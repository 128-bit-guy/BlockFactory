using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using BlockFactory.Network.Packet_;

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
        ((IInGamePacket)packet).Handle(player);
    }

    public void Start()
    {
    }

    public void Dispose()
    {
    }
}
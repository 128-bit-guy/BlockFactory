using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;

namespace BlockFactory.Network.Packet_;

public interface IInGamePacket : IPacket
{
    void Handle(PlayerEntity? sender);
}
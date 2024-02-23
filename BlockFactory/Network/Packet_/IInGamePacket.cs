using BlockFactory.Entity_;

namespace BlockFactory.Network.Packet_;

public interface IInGamePacket : IPacket
{
    void Handle(PlayerEntity? sender);
}
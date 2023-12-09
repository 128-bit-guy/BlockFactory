using BlockFactory.Entity_;
using BlockFactory.Serialization;

namespace BlockFactory.Network;

public interface IPacket : IBinarySerializable
{
    void Handle(PlayerEntity? sender);
    bool SupportsLogicalSide(LogicalSide side);
}
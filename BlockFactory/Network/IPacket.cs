using BlockFactory.Entity_;
using BlockFactory.Serialization;

namespace BlockFactory.Network;

public interface IPacket : IBinarySerializable
{
    bool SupportsLogicalSide(LogicalSide side);
}
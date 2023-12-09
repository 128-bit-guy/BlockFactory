using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Network.Packet_;

[PacketFlags(0)]
public class PlayerPosPacket : IPacket
{
    private Vector3D<double> _pos;

    public PlayerPosPacket(Vector3D<double> pos)
    {
        _pos = pos;
    }

    public PlayerPosPacket() : this(Vector3D<double>.Zero)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        _pos.SerializeBinary(writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _pos.DeserializeBinary(reader);
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.Player.Pos = _pos;
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }
}
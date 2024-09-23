using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Content.Gui.Menu_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class KickPacket : IInGamePacket
{
    private string _message;

    public KickPacket(string message)
    {
        _message = message;
    }

    public KickPacket() : this(string.Empty)
    {
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write(_message);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _message = reader.ReadString();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        while (!BlockFactoryClient.MenuManager.Empty) BlockFactoryClient.MenuManager.Pop();
        BlockFactoryClient.MenuManager.Push(new MainMenu());
        BlockFactoryClient.MenuManager.Push(new KickedMenu(_message));
        BlockFactoryClient.ExitWorld();
    }
}
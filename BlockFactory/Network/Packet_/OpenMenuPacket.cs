using BlockFactory.Client;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Gui.Menu_;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class OpenMenuPacket : IInGamePacket
{
    private DictionaryTag? _guiTag;

    public OpenMenuPacket()
    {
        
    }

    public OpenMenuPacket(SynchronizedMenu menu)
    {
        _guiTag = menu.SerializeToTag(SerializationReason.NetworkInit);
    }
    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        TagIO.Write(_guiTag!, writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        _guiTag = TagIO.Read<DictionaryTag>(reader);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        var menu = new SynchronizedMenu(BlockFactoryClient.Player!);
        menu.DeserializeFromTag(_guiTag!, SerializationReason.NetworkInit);
        BlockFactoryClient.MenuManager.Push(menu);
    }
}
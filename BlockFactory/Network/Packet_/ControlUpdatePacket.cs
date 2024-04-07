using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Gui.Control;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class ControlUpdatePacket : IInGamePacket
{
    private List<int> _path;
    private DictionaryTag? _tag;

    public ControlUpdatePacket(List<int> path)
    {
        _path = path;
    }

    public ControlUpdatePacket(SynchronizedMenuControl control)
        : this(control.GetPathFromMenuManager())
    {
        _tag = control.SerializeToTag(SerializationReason.NetworkUpdate);
    }

    public ControlUpdatePacket() : this(new List<int>())
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write7BitEncodedInt(_path.Count);
        foreach(var x in _path)
        {
            writer.Write7BitEncodedInt(x);
        }

        TagIO.Write(_tag!, writer);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var cnt = reader.Read7BitEncodedInt();
        _path.Clear();
        for (int i = 0; i < cnt; ++i)
        {
            _path.Add(reader.Read7BitEncodedInt());
        }

        _tag = TagIO.Read<DictionaryTag>(reader);
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Client;
    }

    public void Handle(PlayerEntity? sender)
    {
        BlockFactoryClient.MenuManager.GetControlFromPath(_path)?
            .DeserializeFromTag(_tag!, SerializationReason.NetworkUpdate);
    }
}
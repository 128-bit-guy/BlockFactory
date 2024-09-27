using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Content.Gui.Control;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class ControlActionPacket : IInGamePacket
{
    private List<int> _path;
    private int _action;

    public ControlActionPacket(List<int> path, int action)
    {
        _path = path;
        _action = action;
    }

    public ControlActionPacket(SynchronizedMenuControl control, int action)
        : this(control.GetPathFromMenuManager(), action)
    {
        
    }

    public ControlActionPacket() : this(new List<int>(), 0)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write7BitEncodedInt(_path.Count);
        foreach(var x in _path)
        {
            writer.Write7BitEncodedInt(x);
        }
        writer.Write7BitEncodedInt(_action);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var cnt = reader.Read7BitEncodedInt();
        _path.Clear();
        for (int i = 0; i < cnt; ++i)
        {
            _path.Add(reader.Read7BitEncodedInt());
        }

        _action = reader.Read7BitEncodedInt();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }

    public void Handle(PlayerEntity? sender)
    {
        sender!.MenuManager.GetControlFromPath(_path)?.DoAction(_action);
    }
}
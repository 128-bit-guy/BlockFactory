using BlockFactory.Content.Entity_;
using BlockFactory.Content.Gui.Control;
using BlockFactory.Serialization;
using ENet.Managed;

namespace BlockFactory.Network.Packet_;

[PacketFlags(ENetPacketFlags.Reliable)]
public class TextInputTextChangePacket : IInGamePacket
{
    private List<int> _path;
    private string _text;

    public TextInputTextChangePacket(List<int> path, string text)
    {
        _path = path;
        _text = text;
    }

    public TextInputTextChangePacket(TextInputControl control, string text)
        : this(control.GetPathFromMenuManager(), text)
    {
        
    }

    public TextInputTextChangePacket() : this(new List<int>(), string.Empty)
    {
        
    }

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        writer.Write7BitEncodedInt(_path.Count);
        foreach(var x in _path)
        {
            writer.Write7BitEncodedInt(x);
        }
        writer.Write(_text);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        var cnt = reader.Read7BitEncodedInt();
        _path.Clear();
        for (int i = 0; i < cnt; ++i)
        {
            _path.Add(reader.Read7BitEncodedInt());
        }

        _text = reader.ReadString();
    }

    public bool SupportsLogicalSide(LogicalSide side)
    {
        return side == LogicalSide.Server;
    }

    public void Handle(PlayerEntity? sender)
    {
        ((TextInputControl?)sender!.MenuManager.GetControlFromPath(_path))?.SetTextFromPacket(_text);
    }
}
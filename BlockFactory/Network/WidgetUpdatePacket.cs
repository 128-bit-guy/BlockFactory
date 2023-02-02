namespace BlockFactory.Network;

public class WidgetUpdatePacket : IPacket
{
    public readonly byte[] Data;
    public readonly int WidgetIndex;

    public WidgetUpdatePacket(int widgetIndex, byte[] data)
    {
        WidgetIndex = widgetIndex;
        Data = data;
    }

    public WidgetUpdatePacket(BinaryReader reader)
    {
        WidgetIndex = reader.Read7BitEncodedInt();
        var count = reader.Read7BitEncodedInt();
        Data = reader.ReadBytes(count);
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(WidgetIndex);
        writer.Write7BitEncodedInt(Data.Length);
        writer.Write(Data);
    }
}
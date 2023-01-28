namespace BlockFactory.Network;

public class WidgetUpdatePacket : IPacket
{
    public readonly int WidgetIndex;
    public readonly byte[] Data;

    public WidgetUpdatePacket(int widgetIndex, byte[] data)
    {
        WidgetIndex = widgetIndex;
        Data = data;
    }
    public WidgetUpdatePacket(BinaryReader reader)
    {
        WidgetIndex = reader.Read7BitEncodedInt();
        int count = reader.Read7BitEncodedInt();
        Data = reader.ReadBytes(count);
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(WidgetIndex);
        writer.Write7BitEncodedInt(Data.Length);
        writer.Write(Data);
    }
}
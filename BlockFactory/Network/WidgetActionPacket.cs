namespace BlockFactory.Network;

public class WidgetActionPacket : IPacket
{
    public readonly int WidgetIndex;
    public readonly int ActionNumber;

    public WidgetActionPacket(int widgetIndex, int actionNumber)
    {
        WidgetIndex = widgetIndex;
        ActionNumber = actionNumber;
    }
    
    public WidgetActionPacket(BinaryReader reader)
    {
        WidgetIndex = reader.Read7BitEncodedInt();
        ActionNumber = reader.Read7BitEncodedInt();
    }
    
    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(WidgetIndex);
        writer.Write7BitEncodedInt(ActionNumber);
    }
}
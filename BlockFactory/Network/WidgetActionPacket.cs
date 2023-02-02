using BlockFactory.Entity_.Player;
using BlockFactory.Game;

namespace BlockFactory.Network;

public class WidgetActionPacket : IInGamePacket
{
    public readonly int ActionNumber;
    public readonly int WidgetIndex;

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

    public void Process(NetworkConnection connection)
    {
        ((PlayerEntity)connection.SideObject!).Menu!.Widgets[WidgetIndex]
            .ProcessAction(ActionNumber);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend;
    }
}
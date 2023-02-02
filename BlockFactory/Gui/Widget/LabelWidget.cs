using OpenTK.Mathematics;

namespace BlockFactory.Gui.Widget;

public class LabelWidget : InGameMenuWidget
{
    public readonly int Centering;
    public readonly string Text;

    public LabelWidget(InGameMenuWidgetType type, Box2i inclusiveBox, string text, int centering) :
        base(type, inclusiveBox)
    {
        Text = text;
        Centering = centering;
    }

    public LabelWidget(InGameMenuWidgetType type, BinaryReader reader) : base(type, reader)
    {
        Text = reader.ReadString();
        Centering = reader.Read7BitEncodedInt();
    }

    public override void Write(BinaryWriter writer)
    {
        base.Write(writer);
        writer.Write(Text);
        writer.Write7BitEncodedInt(Centering);
    }
}
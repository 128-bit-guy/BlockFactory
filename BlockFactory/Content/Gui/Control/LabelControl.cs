using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Control;

public class LabelControl : SynchronizedMenuControl
{
    [ExclusiveTo(Side.Client)]
    private Box2D<float> _controlBox;
    public string Text;

    public LabelControl(string text)
    {
        Text = text;
    }

    [ExclusiveTo(Side.Client)]
    public override void SetWorkingArea(Box2D<float> box)
    {
        var size = new Vector2D<float>(BfClientContent.TextRenderer.GetStringWidth(Text),
            BfClientContent.TextRenderer.GetStringHeight(Text));
        var min = box.Center - size / 2;
        _controlBox = new Box2D<float>(min, min + size);
    }

    [ExclusiveTo(Side.Client)]
    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(_controlBox.Center.X,
            _controlBox.Center.Y - BfClientContent.TextRenderer.GetStringHeight(Text) / 2, z);
        GuiRenderHelper.RenderText(Text, 0);
        BfRendering.Matrices.Pop();
    }

    [ExclusiveTo(Side.Client)]
    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetValue("text", Text);
        return res;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        Text = tag.GetValue<string>("text");
    }
    
    public override SynchronizedControlType Type => SynchronizedControls.Label;
}
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class LabelControl : MenuControl
{
    private Box2D<float> _controlBox;
    public string Text;

    public LabelControl(string text)
    {
        Text = text;
    }

    public override void SetWorkingArea(Box2D<float> box)
    {
        var size = new Vector2D<float>(BfClientContent.TextRenderer.GetStringWidth(Text),
            BfClientContent.TextRenderer.GetStringHeight(Text));
        var min = box.Center - size / 2;
        _controlBox = new Box2D<float>(min, min + size);
    }

    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(_controlBox.Center.X,
            _controlBox.Center.Y - BfClientContent.TextRenderer.GetStringHeight(Text) / 2, z);
        GuiRenderHelper.RenderText(Text, 0);
        BfRendering.Matrices.Pop();
    }

    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }
}
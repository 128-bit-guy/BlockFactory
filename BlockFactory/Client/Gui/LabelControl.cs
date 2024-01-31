using System.Net.Mime;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class LabelControl : IMenuControl
{
    public string Text;
    private Box2D<float> _controlBox;

    public LabelControl(string text)
    {
        Text = text;
    }

    public void SetWorkingArea(Box2D<float> box)
    {
        var size = new Vector2D<float>(BfClientContent.TextRenderer.GetStringWidth(Text),
            BfClientContent.TextRenderer.GetStringHeight(Text));
        var min = box.Center - (size / 2);
        _controlBox = new Box2D<float>(min, min + size);
    }

    public void UpdateAndRender(float z)
    {
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(_controlBox.Center.X,
            _controlBox.Center.Y - BfClientContent.TextRenderer.GetStringHeight(Text) / 2, z);
        GuiRenderHelper.RenderText(Text, 0);
        BfRendering.Matrices.Pop();
    }

    public Box2D<float> GetControlBox()
    {
        return _controlBox;
    }
}
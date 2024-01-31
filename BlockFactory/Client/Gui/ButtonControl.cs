using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Client.Render.Texture_;
using Silk.NET.Maths;

namespace BlockFactory.Client.Gui;

public class ButtonControl : IMenuControl
{
    private const float Padding = 8f;
    private Box2D<float> _controlBox;
    public string Text;

    public ButtonControl(string text)
    {
        Text = text;
    }

    public void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    public void UpdateAndRender(float z)
    {
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        GuiRenderHelper.RenderQuadWithBorder(Textures.Button, _controlBox, Padding, 1 / 8.0f);
        BfRendering.Matrices.Translate(_controlBox.Center.X,
            _controlBox.Center.Y - BfClientContent.TextRenderer.GetStringHeight(Text) * 0.4f, 1);
        BfRendering.Matrices.Scale(0.8f);
        GuiRenderHelper.RenderText(Text, 0);
        BfRendering.Matrices.Pop();
    }

    public Box2D<float> GetControlBox()
    {
        return _controlBox;
    }
}
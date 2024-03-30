using System.Drawing;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Control;

public class ButtonControl : MenuControl
{
    private const float Padding = 8f;
    private Box2D<float> _controlBox;
    public bool Enabled = true;
    public string Text;

    public ButtonControl(string text)
    {
        Text = text;
    }

    public event Action Pressed = () => { };

    public override void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        var mouseOver = IsMouseOver;
        GuiRenderHelper.RenderQuadWithBorder(
            Enabled ? mouseOver ? 2 : 0 : 1, _controlBox,
            Padding, 1 / 8.0f);
        BfRendering.Matrices.Translate(_controlBox.Center.X,
            _controlBox.Center.Y - BfClientContent.TextRenderer.GetStringHeight(Text) * 0.4f, 1);
        BfRendering.Matrices.Scale(0.8f);
        GuiRenderHelper.RenderText(Text, 0, Enabled && mouseOver ? Color.LightGoldenrodYellow : Color.White);
        BfRendering.Matrices.Pop();
    }

    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        if (IsMouseOver && button == MouseButton.Left && Enabled) Pressed();
    }
}
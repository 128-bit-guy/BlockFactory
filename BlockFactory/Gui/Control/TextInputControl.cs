using System.Drawing;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Control;

public class TextInputControl : MenuControl
{
    private const float Padding = 8f;
    private Box2D<float> _controlBox;
    public int CursorPos;
    public string Text = "";
    public event Action TextChanged = () => { };
    public event Action EnterPressed = () => { };

    public override void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        if (IsMouseOver && button == MouseButton.Left)
            ActiveControl = this;
        else if (ActiveControl == this) ActiveControl = null;
    }

    private string GetRenderedText()
    {
        return ActiveControl == this ? Text.Insert(CursorPos, "|") : Text;
    }

    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        GuiRenderHelper.RenderQuadWithBorder(4, _controlBox, Padding, 1 / 8.0f);
        var renderedText = GetRenderedText();
        BfRendering.Matrices.Translate(_controlBox.Center.X,
            _controlBox.Center.Y - BfClientContent.TextRenderer.GetStringHeight(renderedText) * 0.4f, 1);
        BfRendering.Matrices.Scale(0.8f);
        GuiRenderHelper.RenderText(renderedText, 0, Color.White);
        BfRendering.Matrices.Pop();
    }

    public override void KeyDown(Key key, int a)
    {
        base.KeyDown(key, a);
        if (ActiveControl == this)
            switch (key)
            {
                case Key.Backspace:
                    if (CursorPos != 0)
                    {
                        Text = Text.Remove(CursorPos - 1, 1);
                        --CursorPos;
                        TextChanged();
                    }

                    break;
                case Key.Left:
                    if (CursorPos != 0) --CursorPos;
                    break;
                case Key.Right:
                    if (CursorPos != Text.Length) ++CursorPos;
                    break;
                case Key.Delete:
                    if (CursorPos != Text.Length)
                    {
                        Text = Text.Remove(CursorPos, 1);
                        TextChanged();
                    }

                    break;
                case Key.Enter:
                    EnterPressed();
                    break;
            }
    }

    public override void KeyChar(char c)
    {
        base.KeyChar(c);
        if (ActiveControl == this)
        {
            Text = Text.Insert(CursorPos, "" + c);
            ++CursorPos;
            TextChanged();
        }
    }
}
using System.Drawing;
using System.Net.Mime;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Network.Packet_;
using BlockFactory.Serialization;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Gui.Control;

public class TextInputControl : SynchronizedMenuControl
{
    [ExclusiveTo(Side.Client)]
    private const float Padding = 8f;
    [ExclusiveTo(Side.Client)]
    private Box2D<float> _controlBox;
    [ExclusiveTo(Side.Client)]
    public int CursorPos;
    public string Text = "";
    public event Action TextChanged = () => { };
    public event Action EnterPressed = () => { };

    [ExclusiveTo(Side.Client)]
    public override void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    [ExclusiveTo(Side.Client)]
    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    [ExclusiveTo(Side.Client)]
    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        if (IsMouseOver && button == MouseButton.Left)
            ActiveControl = this;
        else if (ActiveControl == this) ActiveControl = null;
    }

    [ExclusiveTo(Side.Client)]
    private string GetRenderedText()
    {
        return ActiveControl == this ? Text.Insert(CursorPos, "|") : Text;
    }

    [ExclusiveTo(Side.Client)]
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

    [ExclusiveTo(Side.Client)]
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
                        TextChanged0();
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
                        TextChanged0();
                    }

                    break;
                case Key.Enter:
                    DoAction(0);
                    break;
            }
    }

    [ExclusiveTo(Side.Server)]
    public void SetTextFromPacket(string text)
    {
        Text = text;
        TextChanged0();
    }

    private void TextChanged0()
    {
        TextChanged();
        if (LogicalSide == LogicalSide.Client)
        {
            BlockFactoryClient.LogicProcessor!.NetworkHandler
                .SendPacket(null, new TextInputTextChangePacket(this, Text));
        }
    }

    protected override void ProcessAction(int action)
    {
        base.ProcessAction(action);
        EnterPressed();
    }

    [ExclusiveTo(Side.Client)]
    public override void KeyChar(char c)
    {
        base.KeyChar(c);
        if (ActiveControl == this)
        {
            Text = Text.Insert(CursorPos, "" + c);
            ++CursorPos;
            TextChanged0();
        }
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
    
    public override SynchronizedControlType Type => SynchronizedControls.TextInput;
}
using System.Drawing;
using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using BlockFactory.Serialization;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Control;

public class ButtonControl : SynchronizedMenuControl
{
    [ExclusiveTo(Side.Client)]
    private const float Padding = 8f;
    [ExclusiveTo(Side.Client)]
    private Box2D<float> _controlBox;
    public bool Enabled = true;
    public string Text;

    public ButtonControl(string text)
    {
        Text = text;
    }

    public event Action Pressed = () => { };

    [ExclusiveTo(Side.Client)]
    public override void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    [ExclusiveTo(Side.Client)]
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

    [ExclusiveTo(Side.Client)]
    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    [ExclusiveTo(Side.Client)]
    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        if (IsMouseOver && button == MouseButton.Left && Enabled) Pressed0();
    }

    private void Pressed0()
    {
        DoAction(0);
    }

    protected override void ProcessAction(int action)
    {
        base.ProcessAction(action);
        Pressed();
    }

    public override SynchronizedControlType Type => SynchronizedControls.Button;

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetValue("text", Text);
        res.SetValue("enabled", Enabled);
        return res;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        Text = tag.GetValue<string>("text");
        Enabled = tag.GetValue<bool>("enabled");
    }
}
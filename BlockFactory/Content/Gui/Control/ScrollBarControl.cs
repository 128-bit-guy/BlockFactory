using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Client.Render;
using BlockFactory.Client.Render.Gui;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Content.Gui.Control;

[ExclusiveTo(Side.Client)]
public class ScrollBarControl : MenuControl
{
    private const float Padding = 8f;
    private float _buttonMousePosDelta;
    private Box2D<float> _controlBox;
    private bool _interacting;
    public int CurrentPosition;
    public int Positions;
    public event Action PositionChanged = () => { };

    public override void SetWorkingArea(Box2D<float> box)
    {
        _controlBox = box;
    }

    private float GetButtonVerticalPadding()
    {
        return _controlBox.Size.X * (3 / 32.0f);
    }

    private float GetButtonPositionExtent()
    {
        return _controlBox.Size.Y - _controlBox.Size.X - 2 * GetButtonVerticalPadding();
    }

    private Vector2D<float> GetButtonPosition()
    {
        if (Positions == 1) return _controlBox.Center;

        var progress = (float)CurrentPosition / (Positions - 1);
        var delta = GetButtonPositionExtent() * (progress - 0.5f);
        return _controlBox.Center + new Vector2D<float>(0, delta);
    }

    private Box2D<float> GetButtonQuad()
    {
        var pos = GetButtonPosition();
        var halfExt = _controlBox.Size.X / 2;
        return new Box2D<float>(pos - Vector2D<float>.One * halfExt, pos + Vector2D<float>.One * halfExt);
    }

    private bool IsMouseOverButton()
    {
        return GetButtonQuad().Contains(BlockFactoryClient.InputContext.Mice[0].Position.ToGeneric());
    }

    private void UpdateCurrentPosition()
    {
        if (!_interacting) return;
        var newButtonY = BlockFactoryClient.InputContext.Mice[0].Position.Y + _buttonMousePosDelta;
        var extent = GetButtonPositionExtent();
        var min = _controlBox.Center.Y - extent / 2;
        var newButtonRelY = newButtonY - min;
        var newButtonPosFloat = newButtonRelY / extent * (Positions - 1);
        var newButtonPos = Math.Clamp((int)MathF.Round(newButtonPosFloat), 0, Positions - 1);
        if (CurrentPosition == newButtonPos) return;
        CurrentPosition = newButtonPos;
        PositionChanged();
    }

    public override void UpdateAndRender(float z)
    {
        base.UpdateAndRender(z);
        UpdateCurrentPosition();
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(0, 0, z);
        GuiRenderHelper.RenderQuadWithBorder(4, _controlBox, Padding, 1 / 8.0f);
        BfRendering.Matrices.Translate(0, 0, 1);
        var texture = _interacting || IsMouseOverButton() ? 6 : 5;
        GuiRenderHelper.RenderTexturedQuad(texture, GetButtonQuad());
        BfRendering.Matrices.Pop();
    }

    public override Box2D<float> GetControlBox()
    {
        return _controlBox;
    }

    public override void MouseDown(MouseButton button)
    {
        base.MouseDown(button);
        if (button == MouseButton.Left && IsMouseOverButton())
        {
            _interacting = true;
            _buttonMousePosDelta =
                GetButtonPosition().Y - BlockFactoryClient.InputContext.Mice[0].Position.Y;
        }
    }

    public override void MouseUp(MouseButton button)
    {
        base.MouseUp(button);
        if (button == MouseButton.Left) _interacting = false;
    }
}
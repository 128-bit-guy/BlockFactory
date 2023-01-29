using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using BlockFactory.Gui.Widget;
using BlockFactory.Side_;

namespace BlockFactory.Client.Gui.InGame.Widget_;

[ExclusiveTo(Side.Client)]
public class SlotClientWidget : InGameMenuClientWidget<SlotWidget>
{
    public SlotClientWidget(SlotWidget menuWidget, InGameMenuScreen screen) : base(menuWidget, screen)
    {
        screen.Client.OnMouseButton += OnMouseButton;
    }

    public override unsafe void UpdateAndRender()
    {
        base.UpdateAndRender();
        if (IsMouseOver() &&
            (GLFW.GetMouseButton(Screen.Client.Window, MouseButton.Button1) == InputAction.Press ||
             GLFW.GetMouseButton(Screen.Client.Window, MouseButton.Button2) == InputAction.Press) &&
            (GLFW.GetKey(Screen.Client.Window, Keys.LeftShift) == InputAction.Press ||
            GLFW.GetKey(Screen.Client.Window, Keys.RightShift) == InputAction.Press))
        {
            MenuWidget.ProcessAction(GLFW.GetMouseButton(Screen.Client.Window, MouseButton.Button1)
                                     == InputAction.Press ? 3 : 4);
        }

        Screen.DrawColoredRect(Box, 1f, (0.5f, 0.5f, 0.5f, 1f));
        Screen.DrawColoredRect(new(Box.Min, (Box.Max.X, Box.Min.Y + 5)), 1.5f, (0, 0, 0, 1));
        Screen.DrawColoredRect(new(Box.Min, (Box.Min.X + 5, Box.Max.Y)), 1.5f, (0, 0, 0, 1));
        Screen.DrawColoredRect(new((Box.Min.X, Box.Max.Y - 5), Box.Max), 1.5f, (0, 0, 0, 1));
        Screen.DrawColoredRect(new((Box.Max.X - 5, Box.Min.Y), Box.Max), 1.5f, (0, 0, 0, 1));
        Vector2 center = Box.Center;
        Screen.Client.Matrices.Push();
        Screen.Client.Matrices.Translate(new Vector3(center.X, center.Y, 4f));
        Screen.DrawStack(MenuWidget.Stack);
        Screen.Client.Matrices.Pop();
        if (IsMouseOver())
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Screen.DrawColoredRect(new Box2(Box.Min + new Vector2(5), Box.Max - new Vector2(5)),
                7f, (1f, 1f, 1f, 0.4f));
            GL.Disable(EnableCap.Blend);
        }
    }

    private void OnMouseButton(MouseButton button, InputAction action, KeyModifiers modifiers)
    {
        if ((action == InputAction.Press && (modifiers & KeyModifiers.Shift) == 0) && IsMouseOver())
        {
            int widgetAction = 0;
            if (button == MouseButton.Button2)
            {
                widgetAction |= 1;
            }

            if ((modifiers & KeyModifiers.Shift) != 0)
            {
                widgetAction |= 2;
            }

            MenuWidget.ProcessAction(widgetAction);
        }
    }

    public override void Dispose()
    {
        Screen.Client.OnMouseButton -= OnMouseButton;
        base.Dispose();
    }
}
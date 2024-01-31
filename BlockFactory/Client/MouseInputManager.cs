using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Client;

public static class MouseInputManager
{
    private static Vector2D<float> _lastMousePos;
    public static bool MouseIsEnabled { get; private set; } = true;
    public static Vector2D<float> Delta { get; private set; }

    public static bool ImGuiShouldBeFocused()
    {
        return BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.AltLeft);
    }

    private static bool MouseShouldBeEnabled()
    {
        return ImGuiShouldBeFocused() || !BlockFactoryClient.MenuManager.Empty;
    }

    private static void UpdateEnabled()
    {
        if (MouseShouldBeEnabled())
        {
            if (MouseIsEnabled) return;
            MouseIsEnabled = true;
            BlockFactoryClient.InputContext.Mice[0].Cursor.CursorMode = CursorMode.Normal;
        }
        else if (MouseIsEnabled)
        {
            MouseIsEnabled = false;
            BlockFactoryClient.InputContext.Mice[0].Cursor.CursorMode = CursorMode.Disabled;
        }
    }

    private static void UpdateDelta()
    {
        var nPos = BlockFactoryClient.InputContext.Mice[0].Position.ToGeneric();
        Delta = nPos - _lastMousePos;
        _lastMousePos = nPos;
    }

    public static void MouseDown(IMouse mouse, MouseButton button)
    {
        if (MouseShouldBeEnabled() && !ImGuiShouldBeFocused() && !BlockFactoryClient.MenuManager.Empty)
        {
            BlockFactoryClient.MenuManager.Top!.MouseDown(button);
        }
    }

    public static void Update()
    {
        UpdateEnabled();
        UpdateDelta();
    }
}
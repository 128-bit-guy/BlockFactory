using Silk.NET.Input;
using Silk.NET.Maths;

namespace BlockFactory.Client;

public static class MouseInputManager
{
    private static Vector2D<float> _lastMousePos;
    public static bool MouseIsEnabled { get; private set; } = true;
    public static Vector2D<float> Delta { get; private set; }

    private static bool MouseShouldBeEnabled()
    {
        return BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.AltLeft);
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

    public static void Update()
    {
        UpdateEnabled();
        UpdateDelta();
    }
}
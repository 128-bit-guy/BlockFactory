using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using Silk.NET.Input;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public static class PlayerControlManager
{
    private static double _noWorldInteractionTime = 1;
    public static PlayerControlState ControlState { get; private set; }

    public static void Update(double deltaTime)
    {
        if (BlockFactoryClient.MenuManager.HasAnythingToRender())
        {
            ControlState = 0;
            _noWorldInteractionTime = 1;
            return;
        }

        _noWorldInteractionTime -= deltaTime;
        _noWorldInteractionTime = Math.Max(_noWorldInteractionTime, -1);
        PlayerControlState nState = 0;
        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.W))
            nState |= PlayerControlState.MovingForward;

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.S))
            nState |= PlayerControlState.MovingBackwards;

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.A)) nState |= PlayerControlState.MovingLeft;

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.D)) nState |= PlayerControlState.MovingRight;

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.Space)) nState |= PlayerControlState.MovingUp;

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
            nState |= PlayerControlState.MovingDown;

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.ControlLeft))
            nState |= PlayerControlState.Sprinting;

        if (BlockFactoryClient.InputContext.Mice[0].IsButtonPressed(MouseButton.Left) && _noWorldInteractionTime <= 0)
            nState |= PlayerControlState.Attacking;

        if (BlockFactoryClient.InputContext.Mice[0].IsButtonPressed(MouseButton.Right) && _noWorldInteractionTime <= 0)
            nState |= PlayerControlState.Using;

        ControlState = nState;
    }
}
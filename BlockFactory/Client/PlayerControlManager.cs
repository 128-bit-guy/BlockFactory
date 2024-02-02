using BlockFactory.Entity_;
using Silk.NET.Input;

namespace BlockFactory.Client;

public static class PlayerControlManager
{
    public static PlayerControlState ControlState { get; private set; }
    private static double NoWorldInteractionTime = 1;

    public static void Update(double deltaTime)
    {
        if (!BlockFactoryClient.MenuManager.Empty)
        {
            ControlState = 0;
            NoWorldInteractionTime = 1;
            return;
        }

        NoWorldInteractionTime -= deltaTime;
        NoWorldInteractionTime = Math.Max(NoWorldInteractionTime, -1);
        PlayerControlState nState = 0;
        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.W))
        {
            nState |= PlayerControlState.MovingForward;
        }

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.S))
        {
            nState |= PlayerControlState.MovingBackwards;
        }

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.A))
        {
            nState |= PlayerControlState.MovingLeft;
        }

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.D))
        {
            nState |= PlayerControlState.MovingRight;
        }

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.Space))
        {
            nState |= PlayerControlState.MovingUp;
        }

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.ShiftLeft))
        {
            nState |= PlayerControlState.MovingDown;
        }

        if (BlockFactoryClient.InputContext.Keyboards[0].IsKeyPressed(Key.ControlLeft))
        {
            nState |= PlayerControlState.Sprinting;
        }

        if (BlockFactoryClient.InputContext.Mice[0].IsButtonPressed(MouseButton.Left) && NoWorldInteractionTime <= 0)
        {
            nState |= PlayerControlState.Attacking;
        }

        if (BlockFactoryClient.InputContext.Mice[0].IsButtonPressed(MouseButton.Right) && NoWorldInteractionTime <= 0)
        {
            nState |= PlayerControlState.Using;
        }

        ControlState = nState;
    }
}
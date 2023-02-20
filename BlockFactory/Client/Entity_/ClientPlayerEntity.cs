using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;
using BlockFactory.Side_;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Entity_;

[ExclusiveTo(Side.Client)]
public class ClientPlayerEntity : PlayerEntity
{
    private void SendMotionUpdate()
    {
        GameInstance!.NetworkHandler.GetServerConnection().SendPacket(new MotionStateUpdatePacket(MotionState));
    }

    private void SendHeadRotationUpdate()
    {
        GameInstance!.NetworkHandler.GetServerConnection().SendPacket(new HeadRotationUpdatePacket(HeadRotation));
    }

    protected override void TickInternal()
    {
        var preUpdateMotionState = MotionState;
        MotionState = default;
        if (!BlockFactoryClient.Instance.HasScreen())
        {
            MotionState |= BlockFactoryClient.Instance.IsKeyPressed(Keys.W) ? MotionState.MovingForward : 0;
            MotionState |= BlockFactoryClient.Instance.IsKeyPressed(Keys.S) ? MotionState.MovingBackwards : 0;
            MotionState |= BlockFactoryClient.Instance.IsKeyPressed(Keys.A) ? MotionState.MovingLeft : 0;
            MotionState |= BlockFactoryClient.Instance.IsKeyPressed(Keys.D) ? MotionState.MovingRight : 0;
            MotionState |= BlockFactoryClient.Instance.IsKeyPressed(Keys.Space) ? MotionState.MovingUp : 0;
            MotionState |= BlockFactoryClient.Instance.IsKeyPressed(Keys.LeftShift) ? MotionState.MovingDown : 0;
            MotionState |= BlockFactoryClient.Instance.IsMouseButtonPressed(MouseButton.Button1)
                ? MotionState.Attacking
                : 0;
            MotionState |= BlockFactoryClient.Instance.IsMouseButtonPressed(MouseButton.Button2)
                ? MotionState.Using
                : 0;
        }

        if (GameInstance!.Kind.IsNetworked())
        {
            if (MotionState != preUpdateMotionState) SendMotionUpdate();
            SendHeadRotationUpdate();
        }

        base.TickInternal();
    }

    public ClientPlayerEntity(PlayerInfo? playerInfo) : base(playerInfo)
    {
    }
}
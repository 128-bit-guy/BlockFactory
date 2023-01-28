using OpenTK.Windowing.GraphicsLibraryFramework;
using BlockFactory;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Network;

namespace BlockFactory.Client.Entity_
{
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

            MotionState preUpdateMotionState = MotionState;
            MotionState = default;
            if (!BlockFactoryClient.Instance.HasScreen())
            {
                MotionState.MovingForward = BlockFactoryClient.Instance.IsKeyPressed(Keys.W);
                MotionState.MovingBackwards = BlockFactoryClient.Instance.IsKeyPressed(Keys.S);
                MotionState.MovingLeft = BlockFactoryClient.Instance.IsKeyPressed(Keys.A);
                MotionState.MovingRight = BlockFactoryClient.Instance.IsKeyPressed(Keys.D);
                MotionState.MovingUp = BlockFactoryClient.Instance.IsKeyPressed(Keys.Space);
                MotionState.MovingDown = BlockFactoryClient.Instance.IsKeyPressed(Keys.LeftShift);
                MotionState.Attacking = BlockFactoryClient.Instance.IsMouseButtonPressed(MouseButton.Button1);
                MotionState.Using = BlockFactoryClient.Instance.IsMouseButtonPressed(MouseButton.Button2);
            }
            if (GameInstance!.Kind.IsNetworked())
            {
                if (MotionState != preUpdateMotionState)
                {
                    SendMotionUpdate();
                }
                SendHeadRotationUpdate();
            }
            base.TickInternal();
        }
    }
}

using BlockFactory.Entity_.Player;
using BlockFactory.Network;
using BlockFactory.Server.Entity_;
using BlockFactory.Side_;

namespace BlockFactory.Server.Init;

[ExclusiveTo(Side.Server)]
public static class PacketHandlers
{
    private static void HandleMessage(MessagePacket packet, NetworkConnection connection)
    {
        Console.WriteLine($"[{connection.Socket.RemoteEndPoint}]: {packet.Msg}");
        connection.GameInstance!.EnqueueWork(() =>
        {
            if (packet.Msg.StartsWith('/'))
                BlockFactoryServer.Instance.HandleCommand((PlayerEntity)connection.SideObject!, packet.Msg);
            else
                foreach (var networkConnection in BlockFactoryServer.Instance.Connections)
                    networkConnection.SendPacket(
                        new OtherPlayerMessagePacket(connection.Socket.RemoteEndPoint!.ToString()!, packet.Msg));
        });
    }

    private static void HandleMotionStateUpdate(MotionStateUpdatePacket packet, NetworkConnection connection)
    {
        ((ServerPlayerEntity)connection.SideObject!).MotionState = packet.State;
    }

    private static void HandleHeadRotationUpdate(HeadRotationUpdatePacket packet, NetworkConnection connection)
    {
        ((PlayerEntity)connection.SideObject!).HeadRotation = packet.NewRotation;
    }

    private static void HandlePlayerAction(PlayerActionPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
            {
                ((PlayerEntity)connection.SideObject!).HandlePlayerAction(packet.ActionType, packet.Number);
            }
        );
    }

    private static void HandleWidgetAction(WidgetActionPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            ((PlayerEntity)connection.SideObject!).Menu!.Widgets[packet.WidgetIndex]
                .ProcessAction(packet.ActionNumber);
        });
    }

    public static void Init()
    {
        NetworkRegistry.RegisterHandler<MessagePacket>(HandleMessage);
        NetworkRegistry.RegisterHandler<MotionStateUpdatePacket>(HandleMotionStateUpdate);
        NetworkRegistry.RegisterHandler<HeadRotationUpdatePacket>(HandleHeadRotationUpdate);
        NetworkRegistry.RegisterHandler<PlayerActionPacket>(HandlePlayerAction);
        NetworkRegistry.RegisterHandler<WidgetActionPacket>(HandleWidgetAction);
    }
}
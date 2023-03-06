using BlockFactory.Network;

namespace BlockFactory.Init;

public static class Packets
{
    public static void Init()
    {
        NetworkRegistry.Register<MessagePacket>();
        NetworkRegistry.Register<OtherPlayerMessagePacket>();
        NetworkRegistry.Register<MotionStateUpdatePacket>();
        NetworkRegistry.Register<EntityPosUpdatePacket>();
        NetworkRegistry.Register<PlayerJoinWorldPacket>();
        NetworkRegistry.Register<HeadRotationUpdatePacket>();
        NetworkRegistry.Register<ChunkDataPacket>(true);
        NetworkRegistry.Register<ChunkUnloadPacket>();
        NetworkRegistry.Register<RegistrySyncPacket>();
        NetworkRegistry.Register<BlockChangePacket>();
        NetworkRegistry.Register<PlayerActionPacket>();
        NetworkRegistry.Register<PlayerUpdatePacket>();
        NetworkRegistry.Register<InGameMenuOpenPacket>();
        NetworkRegistry.Register<WidgetUpdatePacket>();
        NetworkRegistry.Register<WidgetActionPacket>();
        NetworkRegistry.Register<CredentialsPacket>();
        NetworkRegistry.Register<EntityStackUpdatePacket>();
        NetworkRegistry.Register<EntityAddedPacket>();
        NetworkRegistry.Register<EntityRemovedPacket>();
        NetworkRegistry.Register<EntityChunkPosUpdatePacket>();
    }
}
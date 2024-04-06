﻿namespace BlockFactory.Network.Packet_;

public static class Packets
{
    public static void Init()
    {
        NetworkRegistry.RegisterPacket<ChunkDataPacket>("");
        NetworkRegistry.RegisterPacket<ChunkUnloadPacket>("");
        NetworkRegistry.RegisterPacket<PlayerPosPacket>("");
        NetworkRegistry.RegisterPacket<PlayerControlPacket>("");
        NetworkRegistry.RegisterPacket<BlockChangePacket>("");
        NetworkRegistry.RegisterPacket<LightChangePacket>("");
        NetworkRegistry.RegisterPacket<ServerTickTimePacket>("");
        NetworkRegistry.RegisterPacket<PlayerDataPacket>("");
        NetworkRegistry.RegisterPacket<RegistryMappingPacket>("");
        NetworkRegistry.RegisterPacket<CredentialsPacket>("");
        NetworkRegistry.RegisterPacket<KickPacket>("");
        NetworkRegistry.RegisterPacket<PlayerUpdatePacket>("");
        NetworkRegistry.RegisterPacket<PlayerActionPacket>("");
        NetworkRegistry.RegisterPacket<OpenMenuPacket>("");
        NetworkRegistry.RegisterPacket<CloseMenuPacket>("");
        NetworkRegistry.RegisterPacket<CloseMenuRequestPacket>("");
        NetworkRegistry.RegisterPacket<OpenMenuRequestPacket>("");
        NetworkRegistry.RegisterPacket<ControlActionPacket>("");
        NetworkRegistry.RegisterPacket<TextInputTextChangePacket>("");
    }

    public static void Lock()
    {
        NetworkRegistry.Lock();
    }
}
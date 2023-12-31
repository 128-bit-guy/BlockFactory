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
    }

    public static void Lock()
    {
        NetworkRegistry.Lock();
    }
}
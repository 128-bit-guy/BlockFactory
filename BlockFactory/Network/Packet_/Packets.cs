namespace BlockFactory.Network.Packet_;

public static class Packets
{
    public static void Init()
    {
        NetworkRegistry.RegisterPacket<ChunkDataPacket>("");
        NetworkRegistry.RegisterPacket<ChunkUnloadPacket>("");
    }

    public static void Lock()
    {
        NetworkRegistry.Lock();
    }
}
using ENet.Managed;

namespace BlockFactory.Network;

public struct PacketSendQueueEntry
{
    public readonly IPacket Packet;
    public readonly ENetPeer Peer;
    public readonly ENetPacketFlags Flags;
    public readonly bool Compressed;
    public readonly int Id;

    public PacketSendQueueEntry(IPacket packet, ENetPeer peer, ENetPacketFlags flags, bool compressed, int id)
    {
        Packet = packet;
        Peer = peer;
        Flags = flags;
        Compressed = compressed;
        Id = id;
    }
}
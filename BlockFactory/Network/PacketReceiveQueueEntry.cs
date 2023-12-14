using ENet.Managed;

namespace BlockFactory.Network;

public struct PacketReceiveQueueEntry
{
    public readonly IPacket? Packet;
    public readonly ENetPeer Peer;
    public readonly int Type;

    public PacketReceiveQueueEntry(IPacket? packet, ENetPeer peer, int type)
    {
        Packet = packet;
        Peer = peer;
        Type = type;
    }
}
using BlockFactory.Network.Packet_;
using BlockFactory.World_;
using ENet.Managed;

namespace BlockFactory.Entity_;

public class ServerPlayerEntity : PlayerEntity
{
    public readonly ENetPeer Peer;

    public ServerPlayerEntity(ENetPeer peer)
    {
        Peer = peer;
    }

    public override void OnChunkBecameVisible(Chunk c)
    {
        base.OnChunkBecameVisible(c);
        World!.LogicProcessor.NetworkHandler.SendPacket(this, new ChunkDataPacket(c.Position, c.Data!));
    }

    public override void OnChunkBecameInvisible(Chunk c)
    {
        base.OnChunkBecameInvisible(c);
        World!.LogicProcessor.NetworkHandler.SendPacket(this, new ChunkUnloadPacket(c.Position));
    }

    public override void Update()
    {
        base.Update();
        World!.LogicProcessor.NetworkHandler.SendPacket(this, new PlayerPosPacket(Pos));
    }
}
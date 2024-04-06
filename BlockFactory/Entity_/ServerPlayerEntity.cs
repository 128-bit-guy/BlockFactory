using BlockFactory.Base;
using BlockFactory.Gui;
using BlockFactory.Network.Packet_;
using BlockFactory.World_;
using ENet.Managed;

namespace BlockFactory.Entity_;

[ExclusiveTo(Side.Server)]
public class ServerPlayerEntity : PlayerEntity
{
    public ENetPeer Peer = default;

    public override MenuManager MenuManager { get; }

    public ServerPlayerEntity()
    {
        MenuManager = new ServerMenuManager(this);
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
}
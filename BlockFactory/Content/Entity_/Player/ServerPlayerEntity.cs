using BlockFactory.Base;
using BlockFactory.Content.Gui;
using BlockFactory.Network.Packet_;
using BlockFactory.World_;
using ENet.Managed;

namespace BlockFactory.Content.Entity_.Player;

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
        World!.LogicProcessor.NetworkHandler.SendPacket(this, new ChunkDataPacket(c.Position, c.Data!, Guid));
        if (c.GetEntity(Guid) != null)
        {
            World!.LogicProcessor.NetworkHandler.SendPacket(this, new AddEntityPacket(c.Position, this));
        }
    }

    public override void OnChunkBecameInvisible(Chunk c)
    {
        base.OnChunkBecameInvisible(c);
        if (c.GetEntity(Guid) != null)
        {
            World!.LogicProcessor.NetworkHandler.SendPacket(this, new RemoveEntityPacket(c.Position, Guid));
        }

        World!.LogicProcessor.NetworkHandler.SendPacket(this, new ChunkUnloadPacket(c.Position));
    }
}
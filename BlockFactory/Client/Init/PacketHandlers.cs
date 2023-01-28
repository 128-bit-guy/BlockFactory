using BlockFactory.Client.Entity_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Gui;
using BlockFactory.Init;
using BlockFactory.Network;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Chunk_;

namespace BlockFactory.Client.Init;

public class PacketHandlers
{
    private static void OnOtherPlayerMessage(OtherPlayerMessagePacket packet, NetworkConnection connection)
    {
        Console.WriteLine("[{0}]: {1}", packet.Player, packet.Msg);
    }

    private static void OnEntityPosUpdate(EntityPosUpdatePacket packet, NetworkConnection connection)
    {
        PlayerEntity entity = BlockFactoryClient.Instance.Player!;
        if (entity != null)
        {
            if (entity.Id == packet.Id)
            {
                connection.GameInstance!.EnqueueWork(() =>
                {
                    entity.SetNewPos(packet.Pos);
                    // entity.Pos = packet.Pos;
                });
            }
        }
    }

    private static void OnPlayerJoinWorld(PlayerJoinWorldPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            if (BlockFactoryClient.Instance.Player != null)
            {
                BlockFactoryClient.Instance.Player.Id = packet.Id;
            }
        });
    }

    private static void OnChunkData(ChunkDataPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            Chunk ch = new Chunk(packet.Data, packet.Pos, BlockFactoryClient.Instance.Player!.World!);
            BlockFactoryClient.Instance.Player!.World!.AddChunk(ch);
            BlockFactoryClient.Instance.Player!.AddVisibleChunk(ch);
        });
    }

    private static void OnChunkUnload(ChunkUnloadPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            BlockFactoryClient.Instance.Player?.RemoveVisibleChunk(packet.Pos);
            BlockFactoryClient.Instance.Player?.World!.RemoveChunk(packet.Pos);
        });
    }

    private static void OnRegistrySync(RegistrySyncPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            SyncedRegistries.Sync(packet.Data);
        });
    }

    private static void OnBlockChange(BlockChangePacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            BlockFactoryClient.Instance.Player!.VisibleChunks[packet.Pos.BitShiftRight(Chunk.SizeLog2)]
                .SetBlockState(packet.Pos, packet.State);
        });
    }

    private static void OnPlayerUpdate(PlayerUpdatePacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            BlockFactoryClient.Instance.Player!.HandlePlayerUpdate(packet.UpdateType, packet.Number);
        });
    }

    private static void OnInGameMenuOpen(InGameMenuOpenPacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            BlockFactoryClient.Instance.Player!.SwitchMenu(packet.CreateMenu());
        });
    }

    private static void OnWidgetUpdate(WidgetUpdatePacket packet, NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
        {
            using Stream stream = new MemoryStream(packet.Data);
            using BinaryReader reader = new BinaryReader(stream);
            InGameMenu menu = BlockFactoryClient.Instance.Player!.Menu!;
            if (packet.WidgetIndex == menu.Widgets.Count)
            {
                menu.ReadUpdateData(reader);
            }
            else
            {
                
                menu.Widgets[packet.WidgetIndex].ReadUpdateData(reader);
            }
        });
    }

    internal static void Init()
    {
        NetworkRegistry.RegisterHandler<OtherPlayerMessagePacket>(OnOtherPlayerMessage);
        NetworkRegistry.RegisterHandler<EntityPosUpdatePacket>(OnEntityPosUpdate);
        NetworkRegistry.RegisterHandler<PlayerJoinWorldPacket>(OnPlayerJoinWorld);
        NetworkRegistry.RegisterHandler<ChunkDataPacket>(OnChunkData);
        NetworkRegistry.RegisterHandler<ChunkUnloadPacket>(OnChunkUnload);
        NetworkRegistry.RegisterHandler<RegistrySyncPacket>(OnRegistrySync);
        NetworkRegistry.RegisterHandler<BlockChangePacket>(OnBlockChange);
        NetworkRegistry.RegisterHandler<PlayerUpdatePacket>(OnPlayerUpdate);
        NetworkRegistry.RegisterHandler<InGameMenuOpenPacket>(OnInGameMenuOpen);
        NetworkRegistry.RegisterHandler<WidgetUpdatePacket>(OnWidgetUpdate);
    }
}
using System.Diagnostics;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Init;
using BlockFactory.Network;
using BlockFactory.Server.Entity_;
using BlockFactory.Server.Game;
using BlockFactory.Server.Network;
using BlockFactory.Side_;
using BlockFactory.Util;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Server;

[ExclusiveTo(Side.Server)]
public class BlockFactoryServer
{
    public static readonly BlockFactoryServer Instance = new();
    public ConnectionAcceptor ConnectionAcceptor = null!;
    public HashSet<NetworkConnection> Connections = new();
    public GameInstance GameInstance = null!;
    public bool ShouldRun;

    private BlockFactoryServer()
    {
    }

    private void InitConnectionAcceptor()
    {
        var x = Console.ReadLine()!;
        if (x.Length == 0) x = "" + Constants.DefaultPort;

        var port = int.Parse(x);
        ConnectionAcceptor = new ConnectionAcceptor(port);
        ConnectionAcceptor.OnAccepted += addConnection =>
        {
            addConnection.GameInstance = GameInstance;
            GameInstance.EnqueueWork(() =>
            {
                ServerPlayerEntity player = new(addConnection);
                addConnection.SideObject = player;
                foreach (var connection in Connections)
                    connection.SendPacket(new OtherPlayerMessagePacket("Server",
                        $"Player {connection.Socket.RemoteEndPoint} joined server"));

                Connections.Add(addConnection);
                GameInstance.World.AddPlayer(player);
                addConnection.OnStop += () =>
                {
                    GameInstance.EnqueueWork(() =>
                    {
                        Connections.Remove(addConnection);
                        GameInstance.World.RemovePlayer(player);
                        foreach (var connection in Connections)
                            connection.SendPacket(new OtherPlayerMessagePacket("Server",
                                $"Player {connection.Socket.RemoteEndPoint} left server"));
                    });
                };
                addConnection.Start();
                addConnection.SendPacket(new RegistrySyncPacket(SyncedRegistries.GetSyncData()));
                addConnection.SendPacket(new PlayerJoinWorldPacket(player.Id));
                SpawnPlayer(player, new Vector3i(0, 0, 0));
            });
        };
        ConnectionAcceptor.Start();
    }

    private void Init()
    {
        CommonInit.Init();
        GameInstance = new GameInstance(GameKind.MultiplayerBackend, Thread.CurrentThread,
            unchecked((int)DateTime.UtcNow.Ticks), Path.GetFullPath("world"))
        {
            NetworkHandler = new MultiplayerBackendNetworkHandler(this),
            SideHandler = new ServerSideHandler(this)
        };
        InitConnectionAcceptor();
        GameInstance.Init();
        ShouldRun = true;
    }


    private void Update()
    {
        GameInstance.Update();
    }

    internal void Run()
    {
        Init();
        while (ShouldRun) Update();

        ConnectionAcceptor.Stop();
        ConnectionAcceptor.Dispose();
        GameInstance.Dispose();
    }

    private void SpawnPlayer(PlayerEntity entity, Vector3i chunkPos)
    {
        foreach (var oChunkPos in PlayerChunkLoading.ChunkOffsets.Select(offset => chunkPos + offset))
        {
            var chunk = GameInstance.World.GetOrLoadChunk(oChunkPos);
            var neighbourhood = chunk.Neighbourhood;
            foreach (var blockPos in chunk.GetInclusiveBox().InclusiveEnumerable())
                if (neighbourhood.GetBlockState(blockPos).Block == Blocks.Air &&
                    neighbourhood.GetBlockState(blockPos + Vector3i.UnitY).Block == Blocks.Air &&
                    neighbourhood.GetBlockState(blockPos - Vector3i.UnitY).Block != Blocks.Air)

                {
                    entity.Pos = new EntityPos(blockPos);
                    entity.Pos += new Vector3(0.5f, 1.6f, 0.5f);
                    return;
                }
        }
    }

    internal void HandleCommand(PlayerEntity player, string cmd)
    {
        var split = cmd.Split(' ');
        if (split[0] == "/speed")
        {
            if (split.Length == 1)
            {
                player.Speed = 0.125f;
            }
            else
            {
                if (float.TryParse(split[1], out var speed)) player.Speed = speed;
            }
        }
        else if (split[0] == "/respawn")
        {
            SpawnPlayer(player, new Vector3i(0, 0, 0));
        }
        else if (split[0] == "/toisland")
        {
            var chunkPos = player.Pos.ChunkPos;
            var islandPos = chunkPos;
            islandPos.Y = 100 / 16;
            SpawnPlayer(player, islandPos);
        }
        else if (split[0] == "/dighole")
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var world = player.World!;
            foreach (var offset in new Box3i(
                         new Vector3i(-10, -1000, -10),
                         new Vector3i(10)
                     ).InclusiveEnumerable())
            {
                var blockPos = player.Pos.GetBlockPos() + offset;
                var chunkPos = blockPos.BitShiftRight(Chunk.SizeLog2);
                world.GetOrLoadChunk(chunkPos, false);
            }

            stopwatch.Stop();
            Console.WriteLine($"Scheduling chunks: {stopwatch.Elapsed.TotalMilliseconds}");
            stopwatch.Restart();
            world.Generator.ProcessScheduled();
            stopwatch.Stop();
            Console.WriteLine($"Generating chunks: {stopwatch.Elapsed.TotalMilliseconds}");
            world.Generator.ChunksUpgraded = 0;
            stopwatch.Restart();
            foreach (var offset in new Box3i(
                         new Vector3i(-10, -1000, -10),
                         new Vector3i(10)
                     ).InclusiveEnumerable())
            {
                var blockPos = player.Pos.GetBlockPos() + offset;
                world.SetBlockState(blockPos, new BlockState(Blocks.Air, CubeRotation.Rotations[0]));
            }

            stopwatch.Stop();
            Console.WriteLine($"Placing blocks: {stopwatch.Elapsed.TotalMilliseconds}");
            Console.WriteLine($"Chunks upgraded when placing blocks: {world.Generator.ChunksUpgraded}");
        }
        else if (split[0] == "/printstats")
        {
            Console.WriteLine($"Chunk upgrade on other thread ratio {player.World!.Generator.OtherThreadRatio}");
        }
    }
}
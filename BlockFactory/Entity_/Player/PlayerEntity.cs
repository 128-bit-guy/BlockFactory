using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Game;
using BlockFactory.Gui;
using BlockFactory.Init;
using BlockFactory.Inventory_;
using BlockFactory.Item_;
using BlockFactory.Network;
using BlockFactory.Physics;
using BlockFactory.Serialization.Automatic;
using BlockFactory.Util.Dependency;
using BlockFactory.Util.Math_;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_.Player;

public class PlayerEntity : WalkingEntity
{
    public delegate void MenuChangeHandler(InGameMenu? previous, InGameMenu? current);

    public delegate void VisibleBlockChangeHandler(Chunk chunk, Vector3i pos, BlockState oldState,
        BlockState newState);

    [NotSerialized] private readonly List<Vector3i> _chunksToRemove = new();

    // [NotSerialized] private readonly List<Chunk> _scheduledChunksBecameVisible;

    [NotSerialized] public readonly Dictionary<Vector3i, Chunk> InvDepChunks = new();

    public readonly PlayerInfo? PlayerInfo;

    [NotSerialized] public readonly Dictionary<Vector3i, Chunk> VisibleChunks = new();

    private int _useCooldown;
    public int ChunkLoadDistance = 16;

    public int HotbarPos;
    public MotionState MotionState;
    public float Speed;

    public PlayerEntity(PlayerInfo? playerInfo)
    {
        PlayerInfo = playerInfo;
        if (playerInfo != null) playerInfo.Player = this;
        Inventory = new SimpleInventory(3 * 9);
        for (var i = 0; i < Items.Registry.GetRegisteredEntries().Count; ++i)
            Inventory.TryInsertStack(i, new ItemStack(Items.Registry[i], 64), false);
        Hotbar = new SimpleInventory(9);
        Speed = 0.2f;
        // _scheduledChunksBecameVisible = new List<Chunk>();
    }

    [NotSerialized] public static int MaxHotbarPos => 9;

    [NotSerialized] public InGameMenu? Menu { get; private set; }

    [NotSerialized] public SimpleInventory Inventory { get; }

    [NotSerialized] public SimpleInventory Hotbar { get; }

    [NotSerialized] public ItemStack StackInHand = ItemStack.Empty;

    public event World.ChunkEventHandler ChunkBecameVisible = _ => { };
    public event World.ChunkEventHandler ChunkBecameInvisible = _ => { };

    public event VisibleBlockChangeHandler OnVisibleBlockChange = (_, _, _, _) => { };

    public event MenuChangeHandler OnMenuChange = (_, _) => { };

    public override EntityType Type => Entities.Player;

    protected override void TickInternal()
    {
        base.TickInternal();
        if (_useCooldown > 0) --_useCooldown;
        if (GameInstance!.Kind.DoesProcessLogic())
        {
            if ((MotionState & MotionState.MovingForward) != 0)
                TargetWalkVelocity += GetForward().Xz * (float)Constants.TickPeriod.TotalSeconds;
            if ((MotionState & MotionState.MovingBackwards) != 0)
                TargetWalkVelocity -= GetForward().Xz * (float)Constants.TickPeriod.TotalSeconds;
            if ((MotionState & MotionState.MovingLeft) != 0)
                TargetWalkVelocity -= GetRight().Xz * (float)Constants.TickPeriod.TotalSeconds;
            if ((MotionState & MotionState.MovingRight) != 0)
                TargetWalkVelocity += GetRight().Xz * (float)Constants.TickPeriod.TotalSeconds;

            if (TargetWalkVelocity.LengthSquared > 0.0001f)
            {
                TargetWalkVelocity.Normalize();
                TargetWalkVelocity *= Speed;
            }

            if (IsStandingOnGround)
                if ((MotionState & MotionState.MovingUp) != 0)
                    AddForce(Vector3.UnitY * 0.3f);

            if ((MotionState & (MotionState.Using | MotionState.Attacking)) != 0)
            {
                var rayCastRes = RayCaster.RayCastBlocks(Pos, GetForward() * 10f, Chunk!.Neighbourhood);
                if ((MotionState & MotionState.Using) != 0 && _useCooldown == 0)
                {
                    var stack = GetStackInHand();
                    stack.Item.OnUse(new SlotPointer(Hotbar, HotbarPos), this, rayCastRes);
                }

                if (rayCastRes.HasValue)
                {
                    var (blockPos, time, dir) = rayCastRes.Value;

                    if ((MotionState & MotionState.Attacking) != 0)
                    {
                        Random rng = GameInstance.Random;
                        var state = Chunk!.Neighbourhood.GetBlockState(blockPos);
                        var entity = new ItemEntity(new ItemStack(Items.BlockItems[state.Block]))
                        {
                            Pos = new EntityPos(blockPos) +
                                  new Vector3(rng.NextSingle(), rng.NextSingle(), rng.NextSingle()),
                            Velocity = RandomUtils.PointOnSphere(rng) * 0.1f
                        };
                        entity.Pos.Fix();
                        Chunk!.Neighbourhood.AddEntity(entity);
                        Chunk!.Neighbourhood.SetBlockState(blockPos,
                            new BlockState(Blocks.Air, CubeRotation.Rotations[0]));
                    }
                }
            }

            PickUpItems();

            if (Menu != null) Menu.Update();

            var stackInHand = GetStackInHand();
            if (stackInHand != StackInHand)
            {
                StackInHand = stackInHand;
                if (GameInstance.Kind.IsNetworked())
                {
                    GameInstance.NetworkHandler.GetPlayerConnection(this)
                        .SendPacket(new StackInHandUpdatePacket(Id, StackInHand));
                }
            }

            if (GameInstance.Kind.IsNetworked())
            {
                var posUpdates = (from chunk in VisibleChunks.Values
                    from entity in chunk.Data.EntitiesInChunk.Values
                    select (entity.Chunk!.Pos, entity.Id, entity.Pos)).ToList();
                World!.GameInstance.NetworkHandler.GetPlayerConnection(this)
                    .SendPacket(new EntityPosUpdatePacket(posUpdates));
            }
        }
    }

    private void PickUpItems()
    {
        var bb = GetBoundingBox();
        bb.Min -= new Vector3(1);
        bb.Max += new Vector3(1);
        var e = Chunk!.Neighbourhood.GetInBoxEntityEnumerable(Pos, bb).OfType<ItemEntity>()
            .Where(e => e.PickUpDelay == 0).ToList();
        foreach (var item in e)
        {
            item.Chunk!.RemoveEntity(item);
            var stack = Inventory.Insert(item.Stack, false);
            Hotbar.Insert(stack, false);
        }
    }

    public void AddUseCooldown(int cooldown)
    {
        _useCooldown += cooldown;
    }

    private void OnVisibleChunkAdded(Chunk chunk)
    {
        VisibleChunks.Add(chunk.Pos, chunk);
        if (GameInstance!.Kind.IsNetworked() && GameInstance.Kind.DoesProcessLogic())
            GameInstance.NetworkHandler.GetPlayerConnection(this)
                .SendPacket(new ChunkDataPacket(chunk.Pos, chunk.Data!.ConvertForSending(this)));
        chunk.ViewingPlayers.Add(Id, this);
        ChunkBecameVisible(chunk);
    }

    private void OnVisibleChunkRemoved(Chunk chunk)
    {
        if (GameInstance!.Kind.IsNetworked() && GameInstance.Kind.DoesProcessLogic())
            GameInstance.NetworkHandler.GetPlayerConnection(this).SendPacket(new ChunkUnloadPacket(chunk.Pos));
        chunk.ViewingPlayers.Remove(Id);
        ChunkBecameInvisible(chunk);
    }

    private static void AddDependencyChunk(Chunk chunk)
    {
        // for (var i = -1; i <= 1; ++i)
        // {
        //     for (var j = -1; j <= 1; ++j)
        //     {
        //         for (var k = -1; k <= 1; ++k)
        //         {
        //             var neighbour = chunk.Neighbourhood.Chunks[i + 1, j + 1, k + 1] ??
        //                             chunk.World.GetOrLoadChunk(chunk.Pos + new Vector3i(i, j, k));
        //             ((IDependable)neighbour).OnDependencyAdded();
        //         }
        //     }
        // }
        ((IDependable)chunk).OnDependencyAdded();
    }

    private static void RemoveDependencyChunk(Chunk chunk)
    {
        // for (var i = -1; i <= 1; ++i)
        // {
        //     for (var j = -1; j <= 1; ++j)
        //     {
        //         for (var k = -1; k <= 1; ++k)
        //         {
        //             var neighbour = chunk.Neighbourhood.Chunks[i + 1, j + 1, k + 1] ??
        //                             chunk.World.GetOrLoadChunk(chunk.Pos + new Vector3i(i, j, k));
        //             ((IDependable)neighbour).OnDependencyRemoved();
        //         }
        //     }
        // }
        ((IDependable)chunk).OnDependencyRemoved();
    }

    public void LoadChunks()
    {
        var currentChunksScheduled = 0;
        var currentChunksAdded = 0;
        for (var progress = 0; progress < PlayerChunkLoading.MaxPoses[ChunkLoadDistance]; ++progress)
        {
            var chunkPos = Pos.ChunkPos + PlayerChunkLoading.ChunkOffsets[progress];
            if (VisibleChunks.ContainsKey(chunkPos)) continue;
            var c = World!.GetOrLoadChunk(Pos.ChunkPos + PlayerChunkLoading.ChunkOffsets[progress]);
            if (!(c.GenerationTask == null || c.GenerationTask.IsCompleted))
            {
                ++currentChunksScheduled;
                if (currentChunksScheduled >= 20) break;
            }

            var scheduledContains = InvDepChunks.ContainsKey(chunkPos);
            if (scheduledContains) continue;
            InvDepChunks.Add(chunkPos, c);
            AddDependencyChunk(c);
            ++currentChunksAdded;
            if (currentChunksAdded >= 50) break;
        }

        foreach (var (chunkPos, c) in InvDepChunks)
        {
            if (!c.Generated) continue;
            if (!c.Data.Decorated) continue;
            _chunksToRemove.Add(chunkPos);
            OnVisibleChunkAdded(c);
            // _scheduledChunksBecameVisible.Add(VisibleChunks[chunkPos] = c);
        }

        foreach (var chunkPos in _chunksToRemove) InvDepChunks.Remove(chunkPos);
        _chunksToRemove.Clear();
    }

    public void ProcessScheduledAddedVisibleChunks()
    {
        // foreach (var chunk in _scheduledChunksBecameVisible) OnVisibleChunkAdded(chunk);
        // _scheduledChunksBecameVisible.Clear();
    }

    private bool IsChunkTooFar(Vector3i pos)
    {
        return (Pos.ChunkPos - pos).SquareLength() > (ChunkLoadDistance + 2) * (ChunkLoadDistance + 2);
    }

    public void UnloadChunks(bool all)
    {
        foreach (var (pos, chunk) in VisibleChunks)
            if (all || IsChunkTooFar(pos))
            {
                RemoveDependencyChunk(chunk);
                OnVisibleChunkRemoved(chunk);
                _chunksToRemove.Add(pos);
            }

        foreach (var pos in _chunksToRemove) VisibleChunks.Remove(pos);
        _chunksToRemove.Clear();
        foreach (var (pos, chunk) in InvDepChunks)
            if (all || IsChunkTooFar(pos))
            {
                _chunksToRemove.Add(pos);
                RemoveDependencyChunk(chunk);
            }

        foreach (var pos in _chunksToRemove) InvDepChunks.Remove(pos);
        _chunksToRemove.Clear();
    }

    public void AddVisibleChunk(Chunk chunk)
    {
        OnVisibleChunkAdded(chunk);
    }

    public void RemoveVisibleChunk(Vector3i pos)
    {
        var ch = VisibleChunks[pos];
        OnVisibleChunkRemoved(ch);
        VisibleChunks.Remove(pos);
    }

    public override void OnRemoveFromWorld()
    {
        base.OnRemoveFromWorld();
        if (Menu != null)
        {
            Menu.OnClose();
            Menu = null;
        }

        UnloadChunks(true);
    }

    public void VisibleBlockChanged(Chunk chunk, Vector3i pos, BlockState prevState, BlockState newState)
    {
        OnVisibleBlockChange(chunk, pos, prevState, newState);
        if (GameInstance!.Kind.IsNetworked() && GameInstance.Kind.DoesProcessLogic())
        {
            if (!VisibleChunks.ContainsKey(pos.BitShiftRight(Constants.ChunkSizeLog2))) throw new Exception();
            GameInstance.NetworkHandler.GetPlayerConnection(this).SendPacket(new BlockChangePacket(pos, newState));
        }
    }

    public override Box3 GetBoundingBox()
    {
        return new Box3(-0.4f, (MotionState & MotionState.MovingDown) != 0 ? -1.2f : -1.4f, -0.4f, 0.4f, 0.4f,
            0.4f);
    }

    protected override float GetMaxWalkForce()
    {
        return 0.2f;
    }

    private void UpdateHotbarPosIfNecessary()
    {
        if (GameInstance!.Kind.IsNetworked())
            GameInstance.NetworkHandler.GetPlayerConnection(this)
                .SendPacket(new PlayerUpdatePacket(PlayerUpdateType.HotbarPos, HotbarPos));
    }

    public void HandlePlayerUpdate(PlayerUpdateType updateType, int number)
    {
        switch (updateType)
        {
            case PlayerUpdateType.HotbarPos:
                HotbarPos = number;
                break;
        }
    }

    public ItemStack GetStackInHand()
    {
        return Hotbar[HotbarPos];
    }

    public void HandlePlayerAction(PlayerActionType actionType, int number)
    {
        if (GameInstance!.Kind.DoesProcessLogic())
            switch (actionType)
            {
                case PlayerActionType.AddHotbarPos:
                    HotbarPos += number;
                    if (HotbarPos < 0)
                    {
                        HotbarPos %= MaxHotbarPos;
                        HotbarPos += MaxHotbarPos;
                    }

                    if (HotbarPos >= MaxHotbarPos) HotbarPos %= MaxHotbarPos;
                    UpdateHotbarPosIfNecessary();
                    break;
                case PlayerActionType.SetHotbarPos:
                    if (number >= 0 && number < MaxHotbarPos) HotbarPos = number;
                    UpdateHotbarPosIfNecessary();
                    break;
                case PlayerActionType.ChangeMenu:
                    switch (number)
                    {
                        case 0:
                            SwitchMenu(null);
                            break;
                        case 1:
                            SwitchMenu(Menu == null
                                ? new PlayerInventoryMenu(InGameMenuTypes.PlayerInventory, this)
                                : null);
                            break;
                    }

                    break;
            }
        else if (GameInstance.Kind.IsNetworked())
            GameInstance.NetworkHandler.GetServerConnection().SendPacket(new PlayerActionPacket(actionType, number));
    }

    public void SwitchMenu(InGameMenu? menu)
    {
        Menu?.OnClose();
        var previousMenu = Menu;
        Menu = menu;

        if (Menu != null)
        {
            Menu.Owner = this;
            Menu.OnOpen();
        }

        if (GameInstance!.Kind.DoesProcessLogic() && GameInstance.Kind.IsNetworked())
            GameInstance.NetworkHandler.GetPlayerConnection(this).SendPacket(new InGameMenuOpenPacket(menu));

        OnMenuChange(previousMenu, Menu);
    }
}
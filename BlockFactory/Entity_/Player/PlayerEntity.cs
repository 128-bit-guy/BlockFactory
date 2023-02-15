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
using BlockFactory.Util.Dependency;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_.Player;

public class PlayerEntity : WalkingEntity
{
    public delegate void MenuChangeHandler(InGameMenu? previous, InGameMenu? current);

    public delegate void VisibleBlockChangeHandler(Chunk chunk, Vector3i pos, BlockState oldState,
        BlockState newState);

    private readonly List<Vector3i> _chunksToRemove = new();

    private readonly List<Chunk> _scheduledChunksBecameVisible;
    public readonly Dictionary<Vector3i, Chunk> VisibleChunks = new();
    public readonly Dictionary<Vector3i, Chunk> ScheduledChunks = new();
    private int _useCooldown;
    public int ChunkLoadDistance = 16;

    public int HotbarPos;
    public MotionState MotionState;
    public float Speed;

    public PlayerEntity()
    {
        Inventory = new SimpleInventory(3 * 9);
        for (var i = 0; i < Items.Registry.GetRegisteredEntries().Count; ++i)
            Inventory.TryInsertStack(i, new ItemStack(Items.Registry[i], 64), false);
        Hotbar = new SimpleInventory(9);
        Speed = 0.125f;
        _scheduledChunksBecameVisible = new List<Chunk>();
    }

    public static int MaxHotbarPos => Items.Registry.GetRegisteredEntries().Count;

    public InGameMenu? Menu { get; private set; }

    public SimpleInventory Inventory { get; }

    public SimpleInventory Hotbar { get; }
    public event World.ChunkEventHandler ChunkBecameVisible = _ => { };
    public event World.ChunkEventHandler ChunkBecameInvisible = _ => { };

    public event VisibleBlockChangeHandler OnVisibleBlockChange = (_, _, _, _) => { };

    public event MenuChangeHandler OnMenuChange = (_, _) => { };

    protected override void TickInternal()
    {
        base.TickInternal();
        if (_useCooldown > 0) --_useCooldown;
        if (GameInstance!.Kind.DoesProcessLogic())
        {
            if (MotionState.MovingForward)
                TargetWalkVelocity += GetForward().Xz * (float)Constants.TickPeriod.TotalSeconds;
            if (MotionState.MovingBackwards)
                TargetWalkVelocity -= GetForward().Xz * (float)Constants.TickPeriod.TotalSeconds;
            if (MotionState.MovingLeft) TargetWalkVelocity -= GetRight().Xz * (float)Constants.TickPeriod.TotalSeconds;
            if (MotionState.MovingRight) TargetWalkVelocity += GetRight().Xz * (float)Constants.TickPeriod.TotalSeconds;

            if (TargetWalkVelocity.LengthSquared > 0.0001f)
            {
                TargetWalkVelocity.Normalize();
                TargetWalkVelocity *= Speed;
            }

            if (IsStandingOnGround)
                if (MotionState.MovingUp)
                    AddForce(Vector3.UnitY * 0.2f);

            Pos.Fix();
            if (MotionState.Using || MotionState.Attacking)
            {
                var rayCastRes = RayCaster.RayCastBlocks(Pos, GetForward() * 10f, World!);
                if (MotionState.Using && _useCooldown == 0)
                {
                    var stack = GetStackInHand();
                    stack.Item.OnUse(new SimpleStackContainer(stack), this, rayCastRes);
                }

                if (rayCastRes.HasValue)
                {
                    var (blockPos, time, dir) = rayCastRes.Value;

                    if (MotionState.Attacking)
                        World!.SetBlockState(blockPos, new BlockState(Blocks.Air, CubeRotation.Rotations[0]));
                }
            }

            if (Menu != null) Menu.Update();
        }
    }

    public void AddUseCooldown(int cooldown)
    {
        _useCooldown += cooldown;
    }

    private void OnVisibleChunkAdded(Chunk chunk)
    {
        if (GameInstance!.Kind.IsNetworked() && GameInstance.Kind.DoesProcessLogic())
            GameInstance.NetworkHandler.GetPlayerConnection(this)
                .SendPacket(new ChunkDataPacket(chunk.Pos, chunk.Data!.Clone()));
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

    public void LoadChunks()
    {
        var currentChunksScheduled = 0;
        for (var progress = 0; progress < PlayerChunkLoading.MaxPoses[ChunkLoadDistance]; ++progress)
        {
            var chunkPos = Pos.ChunkPos + PlayerChunkLoading.ChunkOffsets[progress];
            if (!VisibleChunks.ContainsKey(chunkPos) && !ScheduledChunks.ContainsKey(chunkPos))
            {
                var c = World!.GetOrLoadChunk(Pos.ChunkPos + PlayerChunkLoading.ChunkOffsets[progress]);
                if (!(c.GenerationTask == null || c.GenerationTask.IsCompleted))
                {
                    ++currentChunksScheduled;
                    if (currentChunksScheduled == 20)
                    {
                        break;
                    }
                }
                ScheduledChunks.Add(chunkPos, c);
                ((IDependable)c).OnDependencyAdded();
            }
        }
        
        var currentChunksBecameVisible = 0;
        foreach (var (chunkPos, c) in ScheduledChunks)
        {
            if (!c.Generated) continue;
            _chunksToRemove.Add(chunkPos);
            ((IDependable)c).OnDependencyRemoved();
            VisibleChunks.Add(chunkPos, c);
            _scheduledChunksBecameVisible.Add(VisibleChunks[chunkPos] = c);
            ++currentChunksBecameVisible;
            if (currentChunksBecameVisible == 50)
            {
                break;
            }
        }

        foreach (var chunkPos in _chunksToRemove)
        {
            ScheduledChunks.Remove(chunkPos);
        }
        _chunksToRemove.Clear();
    }

    public void ProcessScheduledAddedVisibleChunks()
    {
        foreach (var chunk in _scheduledChunksBecameVisible) OnVisibleChunkAdded(chunk);
        _scheduledChunksBecameVisible.Clear();
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
                OnVisibleChunkRemoved(chunk);
                _chunksToRemove.Add(pos);
            }

        foreach (var pos in _chunksToRemove) VisibleChunks.Remove(pos);
        _chunksToRemove.Clear();
        foreach(var (pos, chunk) in ScheduledChunks)
            if (all || IsChunkTooFar(pos))
            {
                _chunksToRemove.Add(pos);
                ((IDependable)chunk).OnDependencyRemoved();
            }
        
        foreach (var pos in _chunksToRemove) ScheduledChunks.Remove(pos);
        _chunksToRemove.Clear();
    }

    public void AddVisibleChunk(Chunk chunk)
    {
        VisibleChunks.Add(chunk.Pos, chunk);
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
        return new Box3(-0.4f, MotionState.MovingDown ? -1.2f : -1.4f, -0.4f, 0.4f, 0.4f,
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
        return new ItemStack(Items.Registry[HotbarPos]);
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
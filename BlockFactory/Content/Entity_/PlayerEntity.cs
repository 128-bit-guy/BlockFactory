using BlockFactory.Base;
using BlockFactory.Client;
using BlockFactory.Content.Block_;
using BlockFactory.Content.Gui;
using BlockFactory.Content.Gui.Menu_;
using BlockFactory.Content.Item_;
using BlockFactory.Content.Item_.Inventory_;
using BlockFactory.CubeMath;
using BlockFactory.Network.Packet_;
using BlockFactory.Physics;
using BlockFactory.Serialization;
using BlockFactory.World_;
using BlockFactory.World_.ChunkLoading;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_;

public abstract class PlayerEntity : WalkingEntity
{
    public readonly PlayerMotionController MotionController;
    private int _blockCooldown;
    public Inventory Inventory;
    public Inventory HotBar;
    public Inventory MenuHand;
    public int HotBarPos = 0;
    public ItemStack StackInHand;
    public ItemStack StackInMenuHand;
    public bool Spawned = false;
    
    public abstract MenuManager MenuManager { get; }

    public PlayerEntity()
    {
        BoundingBox = new Box3D<double>(-0.4d, -1.4d, -0.4d, 0.4d, 0.4d, 0.4d);
        MotionController = new PlayerMotionController(this);
        StackInHand = new ItemStack(Blocks.Sand, 1);
        Inventory = new Inventory(9 * 3);
        HotBar = new Inventory(9);
        MenuHand = new Inventory(1);
        var pos = 0;
        foreach (var item in Items.Registry)
        {
            var instance = item.CreateInstance();
            Inventory.Insert(pos++, new ItemStack(instance, item.GetMaxCount(instance)), false);
        }

        StackInMenuHand = new ItemStack();
    }

    public PlayerChunkLoader? ChunkLoader { get; private set; }
    public PlayerChunkTicker? ChunkTicker { get; private set; }
    public event Action<Chunk> ChunkBecameVisible = _ => { };
    public event Action<Chunk> ChunkBecameInvisible = _ => { };

    private void ProcessInteraction()
    {
        if (_blockCooldown > 0)
        {
            --_blockCooldown;
            return;
        }

        if (MenuManager.Empty && World!.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            var hitOptional = RayCaster.RayCastBlocks(World!, Pos, GetViewForward()
                .As<double>() * 10);
            if (!hitOptional.HasValue) return;
            var (pos, face) = hitOptional.Value;
            if ((MotionController.ClientState.ControlState & PlayerControlState.Attacking) != 0)
            {
                foreach (var stack in World!.GetBlockObj(pos).GetDroppedStacks(new BlockPointer(World, pos)))
                {
                    var s = (ItemStack)stack.Clone();
                    s = InventoryUtils.Insert(HotBar, s, false);
                    InventoryUtils.Insert(Inventory, s, false);
                }
                World!.SetBlock(pos, 0);
                _blockCooldown = 5;
            }

            if ((MotionController.ClientState.ControlState & PlayerControlState.Using) != 0)
            {
                HotBar[HotBarPos].ItemInstance.Item
                    .Use(HotBar[HotBarPos], new BlockPointer(World, pos), face, this);
                _blockCooldown = 5;
            }
        }
    }

    [ExclusiveTo(Side.Client)]
    public Vector3D<double> GetSmoothPos()
    {
        return MotionController.GetSmoothPos(BlockFactoryClient.LogicProcessor!.GetPartialTicks());
    }

    protected override Vector2D<double> GetTargetWalkVelocity()
    {
        var res = Vector3D<double>.Zero;
        Span<Vector3D<double>> s = stackalloc Vector3D<double>[3];
        s[0] = GetRight().As<double>();
        s[1] = Vector3D<double>.Zero;
        s[2] = GetForward().As<double>();
        foreach (var face in CubeFaceUtils.Values())
        {
            if (((int)MotionController.ClientState.ControlState & (1 << (int)face)) == 0) continue;
            res += s[face.GetAxis()] * face.GetSign();
        }

        if (res.LengthSquared <= 1e-5f) return Vector2D<double>.Zero;

        res = Vector3D.Normalize(res);

        if ((MotionController.ClientState.ControlState & PlayerControlState.Sprinting) != 0)
            res *= 0.3f;
        else
            res *= 0.2f;

        return new Vector2D<double>(res.X, res.Z);
    }

    protected override double GetMaxWalkForce()
    {
        return 0.1d;
    }

    public override void UpdateMotion()
    {
        var state = MotionController.ClientState.ControlState;
        if (!MenuManager.Empty)
        {
            MotionController.ClientState.ControlState = 0;
        }

        if (IsInWater() || !HasGravity)
        {
            var targetVerticalVelocity = 0.0d;
            if ((MotionController.ClientState.ControlState & PlayerControlState.MovingUp) != 0)
            {
                targetVerticalVelocity += 0.2d;
            } else if ((MotionController.ClientState.ControlState & PlayerControlState.MovingDown) != 0)
            {
                targetVerticalVelocity -= 0.2d;
            }

            var delta = targetVerticalVelocity - Velocity.Y;
            delta = Math.Clamp(delta, -0.1d, 0.1d);
            Velocity.Y += delta;
        }
        else if ((MotionController.ClientState.ControlState & PlayerControlState.MovingUp) != 0 && IsStandingOnGround)
        {
            Velocity += Vector3D<double>.UnitY * 0.3d;
        }
        base.UpdateMotion();
        MotionController.ClientState.ControlState = state;
    }

    public override void Update()
    {
        if (!Spawned)
        {
            var posOptional = World!.LogicProcessor.WorldData.SpawnPoint;
            if (posOptional.HasValue)
            {
                Spawned = true;
                Pos = posOptional.Value.As<double>() + new Vector3D<double>(0.5);
            }
            else
            {
                return;
            }
        }
        MotionController.Update();
        base.Update();
        if (World!.LogicProcessor.LogicalSide != LogicalSide.Client) UpdateMotion();

        ProcessInteraction();
        ChunkLoader!.Update();
        ChunkTicker!.Update();
        if (World.LogicProcessor.LogicalSide != LogicalSide.Client && MenuManager.Empty)
        {
            var s = MenuHand.Extract(0, int.MaxValue, false);
            s = InventoryUtils.Insert(HotBar, s, false);
            InventoryUtils.Insert(Inventory, s, false);
        } 
        if (World!.LogicProcessor.LogicalSide != LogicalSide.Client
            && (!StackInHand.Equals(HotBar[HotBarPos]) || !StackInMenuHand.Equals(MenuHand[0])))
        {
            StackInHand = (ItemStack)HotBar[HotBarPos].Clone();
            StackInMenuHand = (ItemStack)MenuHand[0].Clone();
            SendUpdateToClient();
        }
        MenuManager.UpdateLogic();
    }

    public void SendUpdateToClient()
    {
        if (World!.LogicProcessor.LogicalSide == LogicalSide.Server)
        {
            World!.LogicProcessor.NetworkHandler.SendPacket(this, new PlayerUpdatePacket(this));
        }
    }

    public void ProcessPlayerAction(PlayerAction action, int delta)
    {
        if (action == PlayerAction.HotBarAdd)
        {
            HotBarPos += delta;
        }
        else
        {
            HotBarPos = delta;
        }

        HotBarPos %= 9;

        if (HotBarPos < 0)
        {
            HotBarPos += 9;
        }

        SendUpdateToClient();
    }

    [ExclusiveTo(Side.Client)]
    public void DoPlayerAction(PlayerAction action, int delta)
    {
        if (World!.LogicProcessor.LogicalSide == LogicalSide.Client)
        {
            World!.LogicProcessor.NetworkHandler.SendPacket(null, new PlayerActionPacket(action, delta));
        }
        else
        {
            ProcessPlayerAction(action, delta);
        }
    }

    protected override void OnRemovedFromWorld()
    {
        while (!MenuManager.Empty)
        {
            MenuManager.Pop();
        }
        ChunkTicker!.Dispose();
        ChunkTicker = null;
        ChunkLoader!.Dispose();
        ChunkLoader = null;
        base.OnRemovedFromWorld();
    }

    protected override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
        ChunkLoader = new PlayerChunkLoader(this);
        ChunkTicker = new PlayerChunkTicker(this);
    }

    public virtual void OnChunkBecameVisible(Chunk c)
    {
        ChunkBecameVisible(c);
    }

    public virtual void OnChunkBecameInvisible(Chunk c)
    {
        ChunkBecameInvisible(c);
    }

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = base.SerializeToTag(reason);
        res.SetValue("hot_bar_pos", HotBarPos);
        if (reason == SerializationReason.Save)
        {
            res.Set("inventory", Inventory.SerializeToTag(reason));
            res.Set("hot_bar", HotBar.SerializeToTag(reason));
            res.Set("menu_hand", MenuHand.SerializeToTag(reason));
            res.SetValue("spawned", Spawned);
        }
        else
        {
            res.Set("stack_in_hand", StackInHand.SerializeToTag(reason));
            res.Set("stack_in_menu_hand", StackInMenuHand.SerializeToTag(reason));
        }
        return res;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        base.DeserializeFromTag(tag, reason);
        HotBarPos = tag.GetValue<int>("hot_bar_pos");
        if (reason == SerializationReason.Save)
        {
            Inventory.DeserializeFromTag(tag.Get<DictionaryTag>("inventory"), reason);
            HotBar.DeserializeFromTag(tag.Get<DictionaryTag>("hot_bar"), reason);
            MenuHand.DeserializeFromTag(tag.Get<DictionaryTag>("menu_hand"), reason);
            Spawned = tag.GetValue<bool>("spawned");
        }
        else
        {
            StackInHand.DeserializeFromTag(tag.Get<DictionaryTag>("stack_in_hand"), reason);
            StackInMenuHand.DeserializeFromTag(tag.Get<DictionaryTag>("stack_in_menu_hand"), reason);
            Spawned = true;
        }
    }

    public void HandleOpenMenuRequest(OpenMenuRequestType requestType)
    {
        if (World!.LogicProcessor.LogicalSide == LogicalSide.Client)
        {
            World!.LogicProcessor.NetworkHandler.SendPacket(this, new OpenMenuRequestPacket(requestType));
            return;
        }
        if(!MenuManager.Empty) return;
        switch (requestType)
        {
            case OpenMenuRequestType.Message:
                if (World.LogicProcessor.LogicalSide == LogicalSide.Server)
                {
                    MenuManager.Push(new MessageMenu(this));
                }

                break;
            case OpenMenuRequestType.Inventory:
                MenuManager.Push(new InventoryMenu(this));
                break;
        }
    }
    
    
}
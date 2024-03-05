using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.World_;
using BlockFactory.World_.ChunkLoading;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public class PlayerEntity : WalkingEntity
{
    public readonly PlayerMotionController MotionController;
    private int _blockCooldown;

    public PlayerEntity()
    {
        BoundingBox = new Box3D<double>(-0.4d, -1.4d, -0.4d, 0.4d, 0.4d, 0.4d);
        MotionController = new PlayerMotionController(this);
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

        if (World!.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            var hitOptional = RayCaster.RayCastBlocks(World!, Pos, GetViewForward()
                .As<double>() * 10);
            if (!hitOptional.HasValue) return;
            var (pos, face) = hitOptional.Value;
            if ((MotionController.ClientState.ControlState & PlayerControlState.Attacking) != 0)
            {
                World!.SetBlock(pos, 0);
                _blockCooldown = 5;
            }

            if ((MotionController.ClientState.ControlState & PlayerControlState.Using) != 0)
            {
                World!.SetBlock(pos + face.GetDelta(), Blocks.Sand);
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
            res *= 0.8f;
        else
            res *= 0.5f;

        return new Vector2D<double>(res.X, res.Z);
    }

    protected override double GetMaxWalkForce()
    {
        return 0.2d;
    }

    public override void UpdateMotion()
    {
        if (IsInWater())
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
    }

    public override void Update()
    {
        MotionController.Update();
        base.Update();
        if (World!.LogicProcessor.LogicalSide != LogicalSide.Client) UpdateMotion();

        ProcessInteraction();
        ChunkLoader!.Update();
        ChunkTicker!.Update();
    }

    protected override void OnRemovedFromWorld()
    {
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
}
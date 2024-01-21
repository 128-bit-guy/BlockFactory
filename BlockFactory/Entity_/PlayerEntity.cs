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

public class PlayerEntity : Entity
{
    public PlayerChunkLoader? ChunkLoader { get; private set; }
    public PlayerChunkTicker? ChunkTicker { get; private set; }
    public readonly PlayerMotionController MotionController;
    private int _blockCooldown = 0;

    public PlayerEntity()
    {
        MotionController = new PlayerMotionController(this);
    }

    private Vector3D<float> CalculateTargetVelocity()
    {
        var res = Vector3D<float>.Zero;
        Span<Vector3D<float>> s = stackalloc Vector3D<float>[3];
        s[0] = GetRight();
        s[1] = Vector3D<float>.UnitY;
        s[2] = GetForward();
        foreach (var face in CubeFaceUtils.Values())
        {
            if (((int)MotionController.ClientState.ControlState & (1 << ((int)face))) == 0) continue;
            res += s[face.GetAxis()] * face.GetSign();
        }

        if (res.LengthSquared <= 1e-5f)
        {
            return Vector3D<float>.Zero;
        }

        res = Vector3D.Normalize(res);

        if ((MotionController.ClientState.ControlState & PlayerControlState.Sprinting) != 0)
        {
            res *= 0.8f;
        }
        else
        {
            res *= 0.5f;
        }

        return res;
    }

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
                World!.SetBlock(pos + face.GetDelta(), Blocks.Leaves);
                _blockCooldown = 5;
            }
        }
    }

    [ExclusiveTo(Side.Client)]
    public Vector3D<double> GetSmoothPos()
    {
        var state = MotionController.PredictServerStateForTick(MotionController.ClientState.MotionTick + 1);
        return state.Pos + BlockFactoryClient.LogicProcessor.GetPartialTicks() * CalculateTargetVelocity().As<double>();
    }

    public void UpdateMotion()
    {
        var motion = CalculateTargetVelocity();
        Pos += motion.As<double>();
    }

    public override void Update()
    {
        MotionController.Update();
        base.Update();
        if (World!.LogicProcessor.LogicalSide != LogicalSide.Client)
        {
            UpdateMotion();
        }

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
    }

    public virtual void OnChunkBecameInvisible(Chunk c)
    {
    }
}
using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.World_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public class PlayerEntity : Entity
{
    public PlayerChunkLoader? ChunkLoader { get; private set; }
    public PlayerControlState ControlState = 0;
    private int BlockCooldown = 0;

    private Vector3D<float> CalculateTargetVelocity()
    {
        var res = Vector3D<float>.Zero;
        Span<Vector3D<float>> s = stackalloc Vector3D<float>[3];
        s[0] = GetRight();
        s[1] = Vector3D<float>.UnitY;
        s[2] = GetForward();
        foreach (var face in CubeFaceUtils.Values())
        {
            if (((int)ControlState & (1 << ((int)face))) == 0) continue;
            res += s[face.GetAxis()] * face.GetSign();
        }

        if (res.LengthSquared <= 1e-5f)
        {
            return Vector3D<float>.Zero;
        }

        res = Vector3D.Normalize(res);

        if ((ControlState & PlayerControlState.Sprinting) != 0)
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
        if (BlockCooldown > 0)
        {
            --BlockCooldown;
            return;
        }
        var hitOptional = RayCaster.RayCastBlocks(World!, Pos, GetViewForward()
            .As<double>() * 10);
        if (!hitOptional.HasValue) return;
        var (pos, face) = hitOptional.Value;
        if ((ControlState & PlayerControlState.Attacking) != 0)
        {
            World!.SetBlock(pos, 0);
            BlockCooldown = 5;
        }

        if ((ControlState & PlayerControlState.Using) != 0)
        {
            World!.SetBlock(pos + face.GetDelta(), Blocks.Leaves);
            BlockCooldown = 5;
        }
    }

    [ExclusiveTo(Side.Client)]
    public Vector3D<double> GetSmoothPos()
    {
        return Pos + BlockFactoryClient.LogicProcessor.GetPartialTicks() * CalculateTargetVelocity().As<double>();
    }

    public override void Update()
    {
        base.Update();
        var motion = CalculateTargetVelocity();
        Pos += motion.As<double>();
        ProcessInteraction();
        ChunkLoader!.Update();
    }

    protected override void OnRemovedFromWorld()
    {
        ChunkLoader!.Dispose();
        ChunkLoader = null;
        base.OnRemovedFromWorld();
    }

    protected override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
        ChunkLoader = new PlayerChunkLoader(this);
    }
}
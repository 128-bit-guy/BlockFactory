using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public class PlayerEntity : Entity
{
    public PlayerChunkLoader? ChunkLoader { get; private set; }
    public PlayerControlState ControlState = 0;

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
        var blockPos =
            new Vector3D<double>(Math.Floor(Pos.X), Math.Floor(Pos.Y), Math.Floor(Pos.Z))
                .As<int>();
        if ((ControlState & PlayerControlState.Attacking) != 0)
        {
            World!.SetBlock(blockPos, 0);
        }
        if ((ControlState & PlayerControlState.Using) != 0)
        {
            World!.SetBlock(blockPos, Blocks.Bricks);
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
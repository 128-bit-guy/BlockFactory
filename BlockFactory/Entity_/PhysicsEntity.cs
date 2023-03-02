using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Game;
using BlockFactory.Physics;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_;

public abstract class PhysicsEntity : Entity
{
    public delegate void BoxConsumer(Box3 box);

    [ThreadStatic]
    private static List<Box3>? _boxes;
    
    private static List<Box3> Boxes => _boxes ??= new List<Box3>();
    public bool IsStandingOnGround;
    public int LastCollidedMask;
    public Vector3 Velocity;

    public virtual bool HasGravity()
    {
        return true;
    }

    public virtual bool HasAirFriction()
    {
        return true;
    }

    public abstract Box3 GetBoundingBox();

    public void AddForce(Vector3 force)
    {
        Velocity += force;
    }

    public Box3 GetOffsetBoundingBox()
    {
        return GetBoundingBox().Add(Pos.PosInChunk);
    }

    protected virtual bool ShouldHaveFriction()
    {
        return (LastCollidedMask & 2) != 0;
    }

    protected override void TickPhysics()
    {
        if (GameInstance!.Kind.DoesProcessLogic())
        {
            if (HasGravity()) AddForce((0f, -0.02714f, 0f));
        
            Vector3i curOffset = default;
            CubeRotation curRotation = null!;
            BoxConsumer consumer = b => { Boxes.Add(curRotation.RotateAroundCenter(b).Add(curOffset.ToVector3())); };
        
            var bb = GetOffsetBoundingBox();
            var broadphase = bb;
            broadphase.Inflate(bb.Min + Velocity);
            broadphase.Inflate(bb.Max + Velocity);
            // broadphase.Inflate(broadphase.Min - (1.0f, 1.0f, 1.0f));
            // broadphase.Inflate(broadphase.Max + (1.0f, 1.0f, 1.0f));
            for (var x = (int)MathF.Floor(broadphase.Min.X); x < (int)MathF.Ceiling(broadphase.Max.X); ++x)
            for (var y = (int)MathF.Floor(broadphase.Min.Y); y < (int)MathF.Ceiling(broadphase.Max.Y); ++y)
            for (var z = (int)MathF.Floor(broadphase.Min.Z); z < (int)MathF.Ceiling(broadphase.Max.Z); ++z)
            {
                var bco = Pos.ChunkPos.BitShiftLeft(Constants.ChunkSizeLog2);
                var bap = bco + new Vector3i(x, y, z);
                var state = Chunk!.Neighbourhood.GetBlockState(bap);
                var block = state.Block;
                curOffset = bap - Pos.ChunkPos.BitShiftLeft(Constants.ChunkSizeLog2);
                curRotation = state.Rotation;
                block.AddCollisionBoxes(Chunk!.Neighbourhood, bap, state, consumer, this);
                // World.GetBlockState(bap).AddCollisionBoxes(bap, Boxes, this);
            }
        
            var (mVelocity, collidedMask) = CollisionSolver.AdjustMovementForCollision(Velocity, bb, Boxes);
            LastCollidedMask = collidedMask;
            Boxes.Clear();
            var newPos = Pos + mVelocity;
            GameInstance.SideHandler.SetEntityPos(this, newPos);
            IsStandingOnGround = Velocity[1] < 0.0f && (collidedMask & 2) != 0;
            for (var i = 0; i < 3; ++i)
                if ((collidedMask & (1 << i)) != 0)
                    Velocity[i] = 0f;
        
            var velocityLength = Velocity.Length;
            if (velocityLength > 1e-5f)
            {
                if (HasAirFriction()) Velocity -= Velocity / velocityLength * MathF.Min(velocityLength, 0.002f);
        
                if (ShouldHaveFriction()) Velocity -= Velocity / velocityLength * MathF.Min(velocityLength, 0.01f);
            }
        }
    }
}
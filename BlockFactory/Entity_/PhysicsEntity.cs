using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Physics;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;
using BlockFactory.Game;

namespace BlockFactory.Entity_;

public abstract class PhysicsEntity : Entity
{
    public Vector3 Velocity;
    private static List<Box3> Boxes = new List<Box3>();
    public int LastCollidedMask;
    public bool IsStandingOnGround;

    public delegate void BoxConsumer(Box3 box);

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
        if (GameInstance.Kind.DoesProcessLogic())
        {
            if (HasGravity())
            {
                AddForce((0f, -0.01357f, 0f));
            }

            Vector3i curOffset = default;
            CubeRotation curRotation = null!;
            BoxConsumer consumer = b => { Boxes.Add(curRotation.RotateAroundCenter(b).Add(curOffset.ToVector3())); };

            Box3 bb = GetOffsetBoundingBox();
            Box3 broadphase = bb;
            broadphase.Inflate(bb.Min + Velocity);
            broadphase.Inflate(bb.Max + Velocity);
            broadphase.Inflate(broadphase.Min - (1.0f, 1.0f, 1.0f));
            broadphase.Inflate(broadphase.Max + (1.0f, 1.0f, 1.0f));
            for (int x = (int)MathF.Floor(broadphase.Min.X); x <= broadphase.Max.X; ++x)
            {
                for (int y = (int)MathF.Floor(broadphase.Min.Y); y <= broadphase.Max.Y; ++y)
                {
                    for (int z = (int)MathF.Floor(broadphase.Min.Z); z <= broadphase.Max.Z; ++z)
                    {
                        Vector3i bco = Pos.ChunkPos.BitShiftLeft(Chunk.SizeLog2);
                        Vector3i bap = bco + new Vector3i(x, y, z);
                        BlockState state = World!.GetBlockState(bap);
                        Block block = state.Block;
                        curOffset = bap - Pos.ChunkPos.BitShiftLeft(Chunk.SizeLog2);
                        curRotation = state.Rotation;
                        block.AddCollisionBoxes(World, bap, state, consumer, this);
                        // World.GetBlockState(bap).AddCollisionBoxes(bap, Boxes, this);
                    }
                }
            }

            (Vector3 mVelocity, int collidedMask) = CollisionSolver.AdjustMovementForCollision(Velocity, bb, Boxes);
            LastCollidedMask = collidedMask;
            Boxes.Clear();
            EntityPos newPos = Pos + mVelocity;
            newPos.Fix();
            GameInstance.SideHandler.SetEntityPos(this, newPos);
            IsStandingOnGround = (Velocity[1] < 0.0f && (collidedMask & 2) != 0);
            for (int i = 0; i < 3; ++i)
            {
                if ((collidedMask & (1 << i)) != 0)
                {
                    Velocity[i] = 0f;
                }
            }

            float velocityLength = Velocity.Length;
            if (velocityLength > 1e-5f)
            {
                if (HasAirFriction())
                {
                    Velocity -= Velocity / velocityLength * MathF.Min(velocityLength, 0.002f);
                }

                if (ShouldHaveFriction())
                {
                    Velocity -= Velocity / velocityLength * MathF.Min(velocityLength, 0.01f);
                }
            }
        }
    }
}
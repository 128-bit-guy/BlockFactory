using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Math_;
using BlockFactory.Physics;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public class PhysicsEntity : Entity
{
    public Vector3D<double> Velocity;
    public Box3D<double> BoundingBox;
    public bool HasGravity = true;
    public bool IsStandingOnGround;
    public int LastCollidedMask;
    public static readonly Vector3D<double> Gravity = -0.02714f * Vector3D<double>.UnitY;

    public virtual void UpdateMotion()
    {
        Velocity += Gravity;
        var offsetBox = BoundingBox.Add(Pos);
        var calcRes = CollisionCalculator.AdjustMovementForCollision(Velocity, offsetBox, World!);
        if(!calcRes.HasValue) return;
        var (movement, mask) = calcRes.Value;
        Pos += movement;
        LastCollidedMask = mask;
        IsStandingOnGround = Velocity[1] < 0.0f && (mask & 2) != 0;
        for (var i = 0; i < 3; ++i)
        {
            if ((mask & (1 << i)) != 0)
            {
                Velocity.SetValue(i, 0);
            }
        }
        
        var velocityLength = Velocity.Length;
        if (velocityLength > 1e-5f)
        {
            Velocity -= Velocity / velocityLength * Math.Min(velocityLength, 0.002f);
        
            if (IsStandingOnGround) Velocity -= Velocity / velocityLength * Math.Min(velocityLength, 0.01f);

            if (IsInWater()) Velocity -= Velocity / velocityLength * Math.Min(velocityLength, 0.05f);
        }
    }

    public bool IsInWater()
    {
        var offsetBox = BoundingBox.Add(Pos);
        for (var x = (int)Math.Floor(offsetBox.Min.X); x < (int)Math.Ceiling(offsetBox.Max.X); ++x)
        for (var y = (int)Math.Floor(offsetBox.Min.Y); y < (int)Math.Ceiling(offsetBox.Max.Y); ++y)
        for (var z = (int)Math.Floor(offsetBox.Min.Z); z < (int)Math.Ceiling(offsetBox.Max.Z); ++z)
        {
            var pos = new Vector3D<int>(x, y, z);
            if(!World!.IsBlockLoaded(pos)) continue;
            var block = World.GetBlock(pos);
            if (block == Blocks.Water.Id)
            {
                return true;
            }
        }

        return false;
    }

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = base.SerializeToTag(reason);
        if (reason != SerializationReason.NetworkUpdate)
        {
            res.SetVector3D("velocity", Velocity);
            res.SetValue("ground", IsStandingOnGround);
        }

        res.SetValue("has_gravity", HasGravity);
        return res;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        base.DeserializeFromTag(tag, reason);
        if (reason != SerializationReason.NetworkUpdate)
        {
            Velocity = tag.GetVector3D<double>("velocity");
            IsStandingOnGround = tag.GetValue<bool>("ground");
        }

        HasGravity = tag.GetValue<bool>("has_gravity");
    }
}
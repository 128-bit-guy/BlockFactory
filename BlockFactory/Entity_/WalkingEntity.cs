using OpenTK.Mathematics;

namespace BlockFactory.Entity_;

public abstract class WalkingEntity : PhysicsEntity
{
    protected Vector2 TargetWalkVelocity = Vector2.Zero;
    protected abstract float GetMaxWalkForce();
    protected override void TickPhysics()
    {
        var currentWalkVelocity = Velocity.Xz;
        var velocityDelta = TargetWalkVelocity - currentWalkVelocity;
        var maxWalkForce = GetMaxWalkForce();
        if (velocityDelta.LengthSquared > maxWalkForce * maxWalkForce)
        {
            velocityDelta = velocityDelta.Normalized() * maxWalkForce;
        }

        AddForce(new Vector3(velocityDelta.X, 0, velocityDelta.Y));
        TargetWalkVelocity = Vector2.Zero;
        base.TickPhysics();
    }
}
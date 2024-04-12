using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_;

public abstract class WalkingEntity : PhysicsEntity
{
    protected abstract Vector2D<double> GetTargetWalkVelocity();
    protected abstract double GetMaxWalkForce();
    public override void UpdateMotion()
    {
        var currentWalkVelocity = new Vector2D<double>(Velocity.X, Velocity.Z);
        var velocityDelta = GetTargetWalkVelocity() - currentWalkVelocity;
        var maxWalkForce = GetMaxWalkForce();
        if (velocityDelta.LengthSquared > maxWalkForce * maxWalkForce)
            velocityDelta = Vector2D.Normalize(velocityDelta) * maxWalkForce;

        Velocity += new Vector3D<double>(velocityDelta.X, 0, velocityDelta.Y);
        
        base.UpdateMotion();
    }
}
using BlockFactory.Entity_;
using BlockFactory.Registry_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Block_;

public class Block : RegistryEntry
{
    public virtual void AddCollisionBoxes(IBlockReader world, Vector3i pos, BlockState state,
        PhysicsEntity.BoxConsumer consumer, PhysicsEntity entity)
    {
        AddRayCastBoxes(world, pos, state, consumer);
    }

    public virtual void AddRayCastBoxes(IBlockReader world, Vector3i pos, BlockState state,
        PhysicsEntity.BoxConsumer consumer)
    {
        consumer(new Box3(0, 0, 0, 1, 1, 1));
    }
}
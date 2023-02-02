using BlockFactory.Entity_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Block_;

public class AirBlock : Block
{
    public override void AddRayCastBoxes(IBlockReader world, Vector3i pos, BlockState state,
        PhysicsEntity.BoxConsumer consumer)
    {
    }
}
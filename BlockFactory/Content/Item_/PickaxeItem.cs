using BlockFactory.Content.Entity_;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Content.Item_;

public class PickaxeItem : Item
{
    public override void Use(ItemStack stack, BlockPointer pointer, CubeFace face, object user)
    {
        bool sneaking = false;
        if (user is PlayerEntity e)
        {
            if ((e.MotionController.ClientState.ControlState & PlayerControlState.MovingDown) != 0)
            {
                sneaking = true;
            }
        }

        if (sneaking)
        {
            var rotation = CubeSymmetry
                .GetFromTo(CubeFace.Bottom, face.GetOpposite(), true)[0];
            for (var i = -1; i <= 1; ++i)
            for (var j = -10000; j <= 0; ++j)
            for (var k = -1; k <= 1; ++k)
            {
                var pos = pointer + new Vector3D<int>(i, j, k) * rotation;
                pos.SetBlock(0);
            }
        }
        else
        {
            var type = pointer.GetBlock();
            var q = new Queue<(BlockPointer pointer, int distance)>();
            q.Enqueue((pointer, 0));
            while (q.Count > 0)
            {
                var (ptr, dist) = q.Dequeue();
                if (dist > 100) break;
                if (ptr.GetBlock() != type) continue;
                ptr.SetBlock(0);
                foreach (var face1 in CubeFaceUtils.Values())
                {
                    q.Enqueue((ptr + face1.GetDelta(), dist + 1));
                }
            }
        }
    }

    public override int GetTexture(ItemStack stack)
    {
        return 1;
    }

    public override bool IsNonStackable()
    {
        return true;
    }
}
using BlockFactory.CubeMath;
using BlockFactory.World_;

namespace BlockFactory.Item_;

public class PickaxeItem : Item
{
    public override void Use(ItemStack stack, BlockPointer pointer, CubeFace face, object user)
    {
        var type = pointer.GetBlock();
        var q = new Queue<(BlockPointer pointer, int distance)>();
        q.Enqueue((pointer, 0));
        while (q.Count > 0)
        {
            var (ptr, dist) = q.Dequeue();
            if(dist > 100) break;
            if(ptr.GetBlock() != type) continue;
            ptr.SetBlock(0);
            foreach (var face1 in CubeFaceUtils.Values())
            {
                q.Enqueue((ptr + face1.GetDelta(), dist + 1));
            }
        }
        
    }

    public override int GetTexture(ItemStack stack)
    {
        return 1;
    }
}
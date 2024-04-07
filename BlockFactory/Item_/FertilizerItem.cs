using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using BlockFactory.World_.Gen;
using Silk.NET.Maths;

namespace BlockFactory.Item_;

public class FertilizerItem : Item
{
    public override void Use(ItemStack stack, BlockPointer pointer, CubeFace face, object user)
    {
        base.Use(stack, pointer, face, user);
        if(face != CubeFace.Top) return;
        if(pointer.GetBlock() != Blocks.Grass.Id) return;
        TreeGenerator.Generate(pointer + new Vector3D<int>(0, 1, 0), Random.Shared);
        stack.Decrement();
    }

    public override int GetTexture(ItemStack stack)
    {
        return 0;
    }
}
using BlockFactory.CubeMath;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Block_;

public class SandBlock : Block
{
    public override int GetTexture(CubeFace face)
    {
        return 10;
    }

    public override void UpdateBlock(BlockPointer pointer)
    {
        var down = pointer - Vector3D<int>.UnitY;
        if (down.GetBlock() == 0 || down.GetBlock() == Blocks.Water.Id)
        {
            down.SetBlock(this);
            pointer.SetBlock(0);
        }
    }
}
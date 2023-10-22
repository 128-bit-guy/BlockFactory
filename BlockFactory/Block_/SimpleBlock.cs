using BlockFactory.Base;
using BlockFactory.CubeMath;

namespace BlockFactory.Block_;

public class SimpleBlock : Block
{
    private readonly int Texture;

    public SimpleBlock(int texture)
    {
        Texture = texture;
    }

    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return Texture;
    }
}
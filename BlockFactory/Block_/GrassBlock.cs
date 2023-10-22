using BlockFactory.Base;
using BlockFactory.CubeMath;

namespace BlockFactory.Block_;

public class GrassBlock : Block
{
    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return face switch
        {
            CubeFace.Top => 3,
            CubeFace.Bottom => 2,
            _ => 4
        };
    }
}
using BlockFactory.CubeMath;

namespace BlockFactory.Block_;

public class ColumnBlock : Block
{
    private readonly int _top;
    private readonly int _bottom;
    private readonly int _side;

    public ColumnBlock(int top, int bottom, int side)
    {
        _top = top;
        _bottom = bottom;
        _side = side;
    }

    public override int GetTexture(CubeFace face)
    {
        return face switch
        {
            CubeFace.Top => _top,
            CubeFace.Bottom => _bottom,
            _ => _side
        };
    }
}
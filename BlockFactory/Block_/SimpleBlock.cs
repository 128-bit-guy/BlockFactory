using BlockFactory.Base;
using BlockFactory.CubeMath;

namespace BlockFactory.Block_;

public class SimpleBlock : Block
{
    private readonly int _texture;
    private readonly Block _worldGenBase;

    public SimpleBlock(int texture, Block? worldGenBase = null)
    {
        _texture = texture;
        worldGenBase ??= this;
        _worldGenBase = worldGenBase;
    }

    [ExclusiveTo(Side.Client)]
    public override int GetTexture(CubeFace face)
    {
        return _texture;
    }

    public override Block GetWorldGenBase()
    {
        return _worldGenBase;
    }
}
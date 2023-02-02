using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Side_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class GrassBlockMesher : IBlockMesher
{
    private int _top, _side, _bottom;

    public void AddTextures(TextureArrayManager textureArrayManager)
    {
        _top = textureArrayManager.AddOrGetImage("BlockFactory.Assets.Textures.grass_top.png");
        _bottom = textureArrayManager.AddOrGetImage("BlockFactory.Assets.Textures.dirt.png");
        _side = textureArrayManager.AddOrGetImage("BlockFactory.Assets.Textures.grass_side.png");
    }

    public int? GetFaceTexture(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction)
    {
        if (direction == Direction.Up) return _top;

        if (direction == Direction.Down) return _bottom;

        var downDirection = state.Rotation * Direction.Down;
        var downPos = blockPos + downDirection.GetOffset();
        if (reader.GetBlockState(downPos).Block == Blocks.Grass) return _top;

        var absoluteDirection = state.Rotation * direction;
        var oPos = downPos + absoluteDirection.GetOffset();
        var oState = reader.GetBlockState(oPos);
        if (oState.Block == Blocks.Grass)
        {
            if (oState.Rotation.Inverse * downDirection != Direction.Up)
                return _top;
            return _side;
        }

        return _side;
    }

    public bool IsSideSolid(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction)
    {
        return true;
    }
}
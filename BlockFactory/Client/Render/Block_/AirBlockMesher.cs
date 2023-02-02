using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Side_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class AirBlockMesher : IBlockMesher
{
    public void AddTextures(TextureArrayManager textureArrayManager)
    {
    }

    public int? GetFaceTexture(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction)
    {
        return null;
    }

    public bool IsSideSolid(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction)
    {
        return false;
    }
}
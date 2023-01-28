using OpenTK.Mathematics;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;

namespace BlockFactory.Client.Render.Block_;

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
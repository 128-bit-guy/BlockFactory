using OpenTK.Mathematics;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Api;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public interface IBlockMesher
{
    void AddTextures(TextureArrayManager textureArrayManager);
    int? GetFaceTexture(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction);
    bool IsSideSolid(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction);
}
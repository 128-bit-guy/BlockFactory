using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Side_;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class PreSetTextureBlockMesher : IBlockMesher
{
    private readonly bool _areSidesSolid;
    private readonly string[] _textures;
    private int[] _textureIndices;

    public PreSetTextureBlockMesher(string[] textures, bool areSidesSolid)
    {
        _textures = textures;
        _areSidesSolid = areSidesSolid;
    }

    public void AddTextures(TextureArrayManager textureArrayManager)
    {
        _textureIndices = _textures.Select(textureArrayManager.AddOrGetImage).ToArray();
    }

    public int? GetFaceTexture(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction)
    {
        return _textureIndices[(int)direction];
    }

    public bool IsSideSolid(IBlockReader reader, Vector3i blockPos, BlockState state, Direction direction)
    {
        return _areSidesSolid;
    }
}
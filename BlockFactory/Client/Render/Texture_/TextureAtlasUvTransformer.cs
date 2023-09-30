using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Texture_;

[ExclusiveTo(Side.Client)]
public class TextureAtlasUvTransformer : IUvTransformer
{
    private readonly TextureAtlas _atlas;
    public int Sprite;

    public TextureAtlasUvTransformer(TextureAtlas atlas)
    {
        _atlas = atlas;
    }
    public Vector2D<float> TransformUv(Vector2D<float> uv)
    {
        var box = _atlas.GetSpriteBox(Sprite);
        var nx = box.Min.X * (1 - uv.X) + box.Max.X * uv.X;
        var ny = box.Min.Y * (1 - uv.Y) + box.Max.Y * uv.Y;
        return new Vector2D<float>(nx, ny);
    }
}
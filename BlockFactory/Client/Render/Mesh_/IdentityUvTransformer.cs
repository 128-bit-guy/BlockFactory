using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public class IdentityUvTransformer : IUvTransformer
{
    public static readonly IdentityUvTransformer Instance = new();

    private IdentityUvTransformer()
    {
    }

    public Vector2D<float> TransformUv(Vector2D<float> uv)
    {
        return uv;
    }
}
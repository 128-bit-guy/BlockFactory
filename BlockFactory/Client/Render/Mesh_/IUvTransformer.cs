using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public interface IUvTransformer
{
    Vector2D<float> TransformUv(Vector2D<float> uv);
}
using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public interface ILightTransformer
{
    Vector2D<float> TransformLight(Vector2D<float> light);
}
using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public class FullBrightLightTransformer : ILightTransformer
{
    public static readonly FullBrightLightTransformer Instance = new();

    private FullBrightLightTransformer()
    {
        
    }
    public Vector2D<float> TransformLight(Vector2D<float> light)
    {
        return new Vector2D<float>(1, 1);
    }
}
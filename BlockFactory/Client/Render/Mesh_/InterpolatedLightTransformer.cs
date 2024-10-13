using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

public class InterpolatedLightTransformer : ILightTransformer
{
    public IBlockAccess World;
    public Vector3D<double> Pos;
    public bool EnableAutoLighting;
    public Vector2D<float> TransformLight(Vector2D<float> light)
    {
        if (EnableAutoLighting)
        {
            var sky = LightInterpolation.GetInterpolatedBrightness(World, Pos, LightChannel.Sky);
            var block = LightInterpolation.GetInterpolatedBrightness(World, Pos, LightChannel.Block);
            return new Vector2D<float>(sky, block);
        }
        else
        {
            return light;
        }
    }
}
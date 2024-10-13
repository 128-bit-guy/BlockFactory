using BlockFactory.World_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

public class BlockLightTransformer : ILightTransformer
{
    public BlockPointer BlockPointer;
    public bool EnableAutoLighting;
    public Vector2D<float> TransformLight(Vector2D<float> light)
    {
        if (EnableAutoLighting)
        {
            var sky = BlockPointer.GetLight(LightChannel.Sky) / 15.0f;
            var block = BlockPointer.GetLight(LightChannel.Block) / 15.0f;
            return new Vector2D<float>(sky, block);
        }
        else
        {
            return light;
        }
    }
}
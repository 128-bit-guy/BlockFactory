using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public struct BlockVertex
{
    [LayoutLocation(0)] [TransformationType(TransformType.Position)]
    public Vector3D<float> Pos;

    [LayoutLocation(1)] [TransformationType(TransformType.Color)]
    public Vector4D<float> Color;

    [LayoutLocation(2)] [TransformationType(TransformType.Uv)]
    public Vector2D<float> Uv;

    public BlockVertex(Vector3D<float> pos, Vector4D<float> color, Vector2D<float> uv)
    {
        Pos = pos;
        Color = color;
        Uv = uv;
    }

    public BlockVertex(float x, float y, float z, float r, float g, float b, float a, float u, float v) :
        this(new Vector3D<float>(x, y, z), new Vector4D<float>(r, g, b, a),
            new Vector2D<float>(u, v))
    {
    }
}
using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public struct GizmoVertex
{
    [LayoutLocation(0)] [TransformationType(TransformType.Position)]
    public Vector3D<float> Pos;

    [LayoutLocation(1)] [TransformationType(TransformType.Color)]
    public Vector4D<float> Color;

    public GizmoVertex(Vector3D<float> pos, Vector4D<float> color)
    {
        Pos = pos;
        Color = color;
    }

    public GizmoVertex(float x, float y, float z, float r, float g, float b, float a) :
        this(new Vector3D<float>(x, y, z), new Vector4D<float>(r, g, b, a))
    {
    }
}
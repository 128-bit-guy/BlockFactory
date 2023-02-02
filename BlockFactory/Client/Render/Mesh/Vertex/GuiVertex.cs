using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Mesh.Vertex;

[ExclusiveTo(Side.Client)]
public struct GuiVertex
{
    [VertexAttribute(0)] public Vector3 Pos;
    [VertexAttribute(1)] public Vector3 Color;
    [VertexAttribute(2)] public Vector2 Uv;

    public GuiVertex(Vector3 pos, Vector3 color, Vector2 uv)
    {
        Pos = pos;
        Color = color;
        Uv = uv;
    }

    public static implicit operator GuiVertex((Vector3 pos, Vector3 color, Vector2 uv) values)
    {
        return new GuiVertex(values.pos, values.color, values.uv);
    }

    public static implicit operator GuiVertex(
        (float X, float Y, float Z, float R, float G, float B, float UvX, float UvY) values)
    {
        return new GuiVertex((values.X, values.Y, values.Z), (values.R, values.G, values.B), (values.UvX, values.UvY));
    }
}
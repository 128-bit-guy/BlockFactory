using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.Mesh.Vertex;

public struct ColorVertex
{
    [VertexAttribute(0)]
    public Vector3 Pos;
    [VertexAttribute(1)]
    public Vector4 Color;

    public ColorVertex(Vector3 pos, Vector4 color)
    {
        Pos = pos;
        Color = color;
    }

    public static implicit operator ColorVertex((Vector3 pos, Vector4 color) values)
    {
        return new ColorVertex(values.pos, values.color);
    }

    public static implicit operator ColorVertex((float X, float Y, float Z, float R, float G, float B, float A) values)
    {
        return new ColorVertex((values.X, values.Y, values.Z), (values.R, values.G, values.B, values.A));
    }
}
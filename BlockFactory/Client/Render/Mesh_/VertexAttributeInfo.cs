using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Mesh_;

public struct VertexAttributeInfo
{
    public readonly uint LayoutLocation;
    public readonly uint Offset;
    public readonly int Count;
    public readonly VertexAttribType Type;

    public VertexAttributeInfo(uint layoutLocation, uint offset, int count, VertexAttribType type)
    {
        LayoutLocation = layoutLocation;
        Offset = offset;
        Count = count;
        Type = type;
    }
}
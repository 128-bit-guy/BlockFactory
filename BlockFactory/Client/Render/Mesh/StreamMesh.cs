using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;

namespace BlockFactory.Client.Render.Mesh;

[ExclusiveTo(Side.Client)]
public class StreamMesh<T> : IDisposable where T : struct
{
    public readonly MeshBuilder<T> Builder;
    private readonly RenderMesh<T> _mesh;

    public StreamMesh(VertexFormat<T> format)
    {
        Builder = new MeshBuilder<T>(format);
        _mesh = new RenderMesh<T>(format);
    }

    public void Flush()
    {
        Builder.Upload(_mesh);
        Builder.Reset();
        _mesh.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose()
    {
        _mesh.Dispose();
    }
}
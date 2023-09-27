using BlockFactory.Base;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public class RenderMesh : IDisposable
{
    private uint _vao;
    private uint _vbo;
    private uint _ibo;
    public uint IndexCount { get; private set; }
    public readonly VertexBufferObjectUsage Usage;

    public RenderMesh(VertexBufferObjectUsage usage = VertexBufferObjectUsage.StaticDraw)
    {
        Usage = usage;
    }

    public void Clear()
    {
        if (IndexCount == 0) return;
        IndexCount = 0;
        BfRendering.Gl.DeleteVertexArray(_vao);
        Span<uint> buffers = stackalloc uint[2];
        buffers[0] = _vbo;
        buffers[1] = _ibo;
        BfRendering.Gl.DeleteBuffers(buffers);
    }

    private void Init<T>() where T : unmanaged
    {
        _vao = BfRendering.Gl.CreateVertexArray();
        Span<uint> buffers = stackalloc uint[2];
        BfRendering.Gl.CreateBuffers(buffers);
        _vbo = buffers[0];
        _ibo = buffers[1];
        VertexInfo<T>.AttachVbo(_vao, _vbo, 0);
        BfRendering.Gl.VertexArrayElementBuffer(_vao, _ibo);
    }

    public void Upload<T>(ReadOnlySpan<T> vertices, ReadOnlySpan<uint> indices) where T : unmanaged
    {
        if (indices.Length == 0)
        {
            Clear();
            return;
        }

        if (IndexCount == 0)
        {
            Init<T>();
        }
        
        BfRendering.Gl.NamedBufferData(_vbo, vertices, Usage);
        BfRendering.Gl.NamedBufferData(_ibo, indices, Usage);
        IndexCount = (uint)indices.Length;
    }

    public void Bind()
    {
        BfRendering.Gl.BindVertexArray(_vao);
    }
    
    public void Dispose()
    {
        Clear();
    }
}
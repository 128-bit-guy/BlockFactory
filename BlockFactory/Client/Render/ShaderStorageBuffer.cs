using System.Net;
using BlockFactory.Base;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class ShaderStorageBuffer : IDisposable
{
    private readonly uint _ubo;

    public ShaderStorageBuffer()
    {
        _ubo = BfRendering.Gl.CreateBuffer();
    }

    public void Upload<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        BfRendering.Gl.NamedBufferData(_ubo, data, VertexBufferObjectUsage.StaticDraw);
    }

    public void Bind(uint point)
    {
        BfRendering.Gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, point, _ubo);
    }

    public void Dispose()
    {
        BfRendering.Gl.DeleteBuffer(_ubo);
    }
}
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.World_;

[ExclusiveTo(Side.Client)]
public class WorldStreamMeshes : IDisposable
{
    private readonly BlockFactoryClient _client;
    public readonly StreamMesh<BlockVertex> Block;

    public WorldStreamMeshes(BlockFactoryClient client)
    {
        _client = client;
        Block = new StreamMesh<BlockVertex>(VertexFormats.Block, client.Matrices);
    }

    public void FlushAll()
    {
        Shaders.Block.Use();
        _client.VpMatrices.Set(Shaders.Block);
        Shaders.Block.SetModel(Matrix4.Identity);
        Block.Flush(false);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        Block.Dispose();
    }
}
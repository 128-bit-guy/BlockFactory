using OpenTK.Mathematics;
using BlockFactory.Block_;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Client.Render.World_;
using BlockFactory.Client.World_;
using BlockFactory.CubeMath;
using BlockFactory.Item_;
using BlockFactory.Util.Math_;
using OpenTK.Graphics.OpenGL4;

namespace BlockFactory.Client.Render;

public class ItemRenderer : IDisposable
{
    public readonly BlockFactoryClient Client;
    public readonly WorldRenderer Renderer;
    public readonly EmptyBlockReader EmptyBlockReader;
    private readonly RenderMesh<BlockVertex> BlockMesh;
    private readonly MeshBuilder<BlockVertex> BlockMeshBuilder;

    public ItemRenderer(BlockFactoryClient client, WorldRenderer renderer)
    {
        Client = client;
        Renderer = renderer;
        EmptyBlockReader = new EmptyBlockReader();
        BlockMesh = new RenderMesh<BlockVertex>(VertexFormats.Block);
        BlockMeshBuilder = new MeshBuilder<BlockVertex>(VertexFormats.Block);
    }

    public void RenderItemStack(ItemStack stack)
    {
        BlockMeshBuilder.MatrixStack.Push();
        if (stack.Item is BlockItem b)
        {
            Renderer.BlockRenderer.RenderBlock(new BlockState(b.Block, CubeRotation.Rotations[0]),
                EmptyBlockReader, new Vector3i(0, 0, 0), BlockMeshBuilder);
        }
        BlockMeshBuilder.MatrixStack.Pop();
        BlockMeshBuilder.Upload(BlockMesh);
        BlockMeshBuilder.Reset();
        Shaders.Block.Use();
        Client.VpMatrices.Set(Shaders.Block);
        Shaders.Block.SetModel(BlockFactoryClient.Instance.Matrices);
        BlockMesh.Bind();
        GL.DrawElements(PrimitiveType.Triangles, BlockMesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        BlockMesh.DeleteGl();
    }
}
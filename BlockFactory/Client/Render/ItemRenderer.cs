using BlockFactory.Block_;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.Client.Render.Shader;
using BlockFactory.Client.Render.World_;
using BlockFactory.Client.World_;
using BlockFactory.CubeMath;
using BlockFactory.Item_;
using BlockFactory.Side_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class ItemRenderer : IDisposable
{
    private readonly StreamMesh<BlockVertex> BlockMesh;
    public readonly BlockFactoryClient Client;
    public readonly EmptyBlockReader EmptyBlockReader;
    public readonly WorldRenderer Renderer;

    public ItemRenderer(BlockFactoryClient client, WorldRenderer renderer)
    {
        Client = client;
        Renderer = renderer;
        EmptyBlockReader = new EmptyBlockReader();
        BlockMesh = new StreamMesh<BlockVertex>(VertexFormats.Block);
    }

    public void Dispose()
    {
        BlockMesh.Dispose();
    }

    public void RenderItemStack(ItemStack stack)
    {
        RenderItemStack(stack, BlockMesh.Builder);
        Shaders.Block.Use();
        Client.VpMatrices.Set(Shaders.Block);
        Shaders.Block.SetModel(BlockFactoryClient.Instance.Matrices);
        BlockMesh.Flush();
        GL.BindVertexArray(0);
    }

    public void RenderItemStack(ItemStack stack, MeshBuilder<BlockVertex> blockMeshBuilder)
    {
        blockMeshBuilder.MatrixStack.Push();
        if (stack.Item is BlockItem b)
            Renderer.BlockRenderer.RenderBlock(new BlockState(b.Block, CubeRotation.Rotations[0]),
                EmptyBlockReader, new Vector3i(0, 0, 0), blockMeshBuilder);
        blockMeshBuilder.MatrixStack.Pop();
    }
}
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

public class DynamicMesh : IDisposable
{
    public readonly BlockMeshBuilder BlockMeshBuilder;
    public readonly ItemMeshBuilder ItemMeshBuilder;
    public readonly RenderMesh BlockMesh;
    public readonly RenderMesh ItemMesh;
    public readonly MatrixStack Matrices;

    public DynamicMesh(MatrixStack? matrices = null)
    {
        Matrices = matrices ?? new MatrixStack();
        BlockMeshBuilder = new BlockMeshBuilder(Matrices);
        ItemMeshBuilder = new ItemMeshBuilder(Matrices);
        BlockMesh = new RenderMesh(VertexBufferObjectUsage.StreamDraw);
        ItemMesh = new RenderMesh(VertexBufferObjectUsage.StreamDraw);
    }

    public void Dispose()
    {
        BlockMesh.Dispose();
        ItemMesh.Dispose();
    }

    public void Render()
    {
        BfRendering.Gl.Enable(EnableCap.Blend);
        BfRendering.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        RenderBlock();
        RenderItem();

        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);

        BfRendering.Gl.Disable(EnableCap.Blend);
    }

    private unsafe void RenderBlock()
    {
        BlockMeshBuilder.MeshBuilder.Upload(BlockMesh);

        BlockMeshBuilder.MeshBuilder.Reset();

        if (BlockMesh.IndexCount == 0) return;

        Shaders.Block.Use();
        Textures.Blocks.Bind();
        BfRendering.SetVpMatrices(Shaders.Block);
        Shaders.Block.SetModel(BfRendering.Matrices);
        Shaders.Block.SetLoadProgress(1);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Blocks.SpriteBoxesBuffer.Bind(2);
        BlockMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, BlockMesh.IndexCount,
            DrawElementsType.UnsignedInt, null);
    }

    private unsafe void RenderItem()
    {
        ItemMeshBuilder.MeshBuilder.Upload(ItemMesh);

        ItemMeshBuilder.MeshBuilder.Reset();

        if (ItemMesh.IndexCount == 0) return;

        BfRendering.Gl.Enable(EnableCap.Blend);
        BfRendering.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Shaders.Block.Use();
        Textures.Items.Bind();
        BfRendering.SetVpMatrices(Shaders.Block);
        Shaders.Block.SetModel(BfRendering.Matrices);
        Shaders.Block.SetLoadProgress(1);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Items.SpriteBoxesBuffer.Bind(2);
        ItemMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, ItemMesh.IndexCount,
            DrawElementsType.UnsignedInt, null);
    }
}
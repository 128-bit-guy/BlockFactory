using BlockFactory.Base;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Content.Block_;
using BlockFactory.Content.Item_;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public static class ItemRenderer
{
    private static BlockMeshBuilder _blockMeshBuilder = null!;
    private static ItemMeshBuilder _itemMeshBuilder = null!;
    private static readonly uint[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
    private static RenderMesh _blockMesh = null!;
    private static RenderMesh _itemMesh = null!;
    private static readonly float[] VertexLight = new float[4];

    public static void Init()
    {
        _blockMeshBuilder = new BlockMeshBuilder();
        _blockMesh = new RenderMesh(VertexBufferObjectUsage.StreamDraw);
        _itemMeshBuilder = new ItemMeshBuilder();
        _itemMesh = new RenderMesh(VertexBufferObjectUsage.StreamDraw);
    }

    public static void Destroy()
    {
        _blockMesh.Dispose();
        _itemMesh.Dispose();
    }

    public static void RenderItemStack(ItemStack stack)
    {
        if (stack.ItemInstance.Item is BlockItem blockItem)
        {
            RenderBlock(blockItem.Block);
        }
        else
        {
            RenderItem(stack);
        }
    }

    private static unsafe void RenderBlock(Block block)
    {
        var builder = _blockMeshBuilder.MeshBuilder;
        var transformer = _blockMeshBuilder.UvTransformer;
        Span<byte> lightVal = stackalloc byte[4];
        if (block == Blocks.Air) return;
        builder.Matrices.Push();
        builder.Matrices.Translate(-0.5f, -0.5f, -0.5f);
        if (block is FenceBlock f)
        {
            BlockMeshes.RenderFence(f, new BlockPointer(EmptyWorld.Instance, Vector3D<int>.Zero), _blockMeshBuilder);
        } else if (block is TorchBlock t)
        {
            BlockMeshes.RenderTorch(t, new BlockPointer(EmptyWorld.Instance, Vector3D<int>.Zero), _blockMeshBuilder);
        } else if (block is TallGrassBlock g)
        {
            BlockMeshes.RenderTallGrass(
                g,
                new BlockPointer(EmptyWorld.Instance, Vector3D<int>.Zero),
                _blockMeshBuilder
                );
        }
        else
        {
            foreach (var face in CubeFaceUtils.Values())
            {
                transformer.Sprite = block.GetTexture(face);
                var s = face.GetAxis() == 1
                    ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                    : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;

                for (var u = 0; u < 2; ++u)
                for (var v = 0; v < 2; ++v)
                {
                    byte cLight = 15;
                    var ao = false;

                    if (ao) cLight -= Math.Min(cLight, (byte)3);
                    lightVal[u | (v << 1)] = cLight;
                }

                var dtMask = 0;
                for (var l = 0; l < 4; ++l)
                {
                    VertexLight[l] = (float)lightVal[l] / 15;
                    dtMask |= lightVal[l] << (l << 2);
                }

                builder.Matrices.Push();
                builder.Matrices.Multiply(s.AroundCenterMatrix4);
                builder.NewPolygon().Indices(QuadIndices)
                    .Vertex(new BlockVertex(0, 0, 1, VertexLight[0], VertexLight[0], VertexLight[0], 1, 0, 0))
                    .Vertex(new BlockVertex(1, 0, 1, VertexLight[1], VertexLight[1], VertexLight[1], 1, 1, 0))
                    .Vertex(new BlockVertex(1, 1, 1, VertexLight[3], VertexLight[3], VertexLight[3], 1, 1, 1))
                    .Vertex(new BlockVertex(0, 1, 1, VertexLight[2], VertexLight[2], VertexLight[2], 1, 0, 1));
                builder.Matrices.Pop();
            }
        }

        builder.Matrices.Pop();

        builder.Upload(_blockMesh);

        builder.Reset();

        if (_blockMesh.IndexCount == 0) return;

        BfRendering.Gl.Enable(EnableCap.Blend);
        BfRendering.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Shaders.Block.Use();
        Textures.Blocks.Bind();
        BfRendering.SetVpMatrices(Shaders.Block);
        Shaders.Block.SetModel(BfRendering.Matrices);
        Shaders.Block.SetLoadProgress(1);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Blocks.SpriteBoxesBuffer.Bind(2);
        _blockMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _blockMesh.IndexCount,
            DrawElementsType.UnsignedInt, null);

        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);

        BfRendering.Gl.Disable(EnableCap.Blend);
    }

    private static unsafe void RenderItem(ItemStack stack)
    {
        var builder = _itemMeshBuilder.MeshBuilder;
        var transformer = _itemMeshBuilder.UvTransformer;
        builder.Matrices.Push();
        builder.Matrices.Translate(-0.5f, -0.5f, 0f);
        {
            transformer.Sprite = stack.ItemInstance.Item.GetTexture(stack);

            builder.Matrices.Push();
            builder.NewPolygon().Indices(QuadIndices)
                .Vertex(new BlockVertex(0, 0, 0, 1.0f, 1.0f, 1.0f, 1, 0, 0))
                .Vertex(new BlockVertex(1, 0, 0, 1.0f, 1.0f, 1.0f, 1, 1, 0))
                .Vertex(new BlockVertex(1, 1, 0, 1.0f, 1.0f, 1.0f, 1, 1, 1))
                .Vertex(new BlockVertex(0, 1, 0, 1.0f, 1.0f, 1.0f, 1, 0, 1));
            builder.Matrices.Pop();
        }

        builder.Matrices.Pop();

        builder.Upload(_itemMesh);

        builder.Reset();

        if (_itemMesh.IndexCount == 0) return;

        BfRendering.Gl.Enable(EnableCap.Blend);
        BfRendering.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Shaders.Block.Use();
        Textures.Items.Bind();
        BfRendering.SetVpMatrices(Shaders.Block);
        Shaders.Block.SetModel(BfRendering.Matrices);
        Shaders.Block.SetLoadProgress(1);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Items.SpriteBoxesBuffer.Bind(2);
        _itemMesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, _itemMesh.IndexCount,
            DrawElementsType.UnsignedInt, null);

        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);

        BfRendering.Gl.Disable(EnableCap.Blend);
    }
}
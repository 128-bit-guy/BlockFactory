using BlockFactory.Base;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Content.Block_;
using BlockFactory.Content.Item_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Light;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public static class ItemRenderer
{
    private static DynamicMesh _dynamicMesh = null!;
    private static readonly uint[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
    private static RenderMesh _blockMesh = null!;
    private static RenderMesh _itemMesh = null!;
    private static readonly float[] VertexLight = new float[4];

    public static void Init()
    {
        _dynamicMesh = new DynamicMesh();
    }

    public static void Destroy()
    {
        _dynamicMesh.Dispose();
    }

    public static void RenderItemStack(ItemStack stack)
    {
        RenderItemStack(stack, _dynamicMesh);
        _dynamicMesh.Render();
    }

    public static void RenderItemStack(ItemStack stack, DynamicMesh mesh)
    {
        if (stack.ItemInstance.Item is BlockItem blockItem)
        {
            RenderBlock(blockItem.Block, mesh);
        }
        else
        {
            RenderItem(stack, mesh);
        }
    }

    private static unsafe void RenderBlock(Block block, DynamicMesh mesh)
    {
        var builder = mesh.BlockMeshBuilder.MeshBuilder;
        var transformer = mesh.BlockMeshBuilder.UvTransformer;
        Span<byte> lightVal = stackalloc byte[4];
        if (block == Blocks.Air) return;
        builder.Matrices.Push();
        builder.Matrices.Translate(-0.5f, -0.5f, -0.5f);
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

        builder.Matrices.Pop();
    }

    private static unsafe void RenderItem(ItemStack stack, DynamicMesh mesh)
    {
        var builder = mesh.ItemMeshBuilder.MeshBuilder;
        var transformer = mesh.ItemMeshBuilder.UvTransformer;
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
    }
}
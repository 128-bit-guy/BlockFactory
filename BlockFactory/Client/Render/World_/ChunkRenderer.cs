using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Side_;
using BlockFactory.World_.Chunk_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.World_;

[ExclusiveTo(Side.Client)]
public class ChunkRenderer : IDisposable
{
    private readonly RenderMesh<BlockVertex> _mesh;
    public readonly Chunk Chunk;
    public readonly ChunkRendererNeighbourhood Neighbourhood;
    public readonly Vector3i Pos;
    public readonly WorldRenderer WorldRenderer;
    public Task<MeshBuilder<BlockVertex>>? RebuildTask;
    public bool RequiresRebuild;

    public ChunkRenderer(Vector3i pos, Chunk chunk, WorldRenderer worldRenderer)
    {
        Pos = pos;
        Chunk = chunk;
        WorldRenderer = worldRenderer;
        Neighbourhood = new ChunkRendererNeighbourhood(pos);
        _mesh = new RenderMesh<BlockVertex>(VertexFormats.Block);
        RequiresRebuild = true;
    }

    public void Dispose()
    {
        _mesh.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool HasAnythingToRender()
    {
        return _mesh.IndexCount > 0;
    }

    public MeshBuilder<BlockVertex> Rebuild()
    {
        MeshBuilder<BlockVertex> mb = new(VertexFormats.Block);
        mb.MatrixStack.Push();
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            Vector3i relativePos = new(i, j, k);
            var absolutePos = Pos.BitShiftLeft(Constants.ChunkSizeLog2) + relativePos;
            var state = Neighbourhood.GetBlockState(absolutePos);
            if (state.Block != Blocks.Air)
            {
                mb.MatrixStack.Push();
                mb.MatrixStack.Translate(relativePos);
                WorldRenderer.BlockRenderer.RenderBlock(state, Neighbourhood, absolutePos, mb);
                mb.MatrixStack.Pop();
            }
        }

        mb.MatrixStack.Pop();
        return mb;
    }

    public void Upload(MeshBuilder<BlockVertex> mb)
    {
        mb.Upload(_mesh);
    }

    public void Render()
    {
        _mesh.Bind();
        GL.DrawElements(PrimitiveType.Triangles, _mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
    }
}
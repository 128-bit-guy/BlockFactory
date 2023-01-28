using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Block_;
using BlockFactory.Client.Render.Mesh;
using BlockFactory.Client.Render.Mesh.Vertex;
using BlockFactory.CubeMath;
using BlockFactory.Init;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Chunk_;

namespace BlockFactory.Client.Render.World_
{
    public class ChunkRenderer : IDisposable
    {
        public readonly Vector3i Pos;
        public readonly ChunkRendererNeighbourhood Neighbourhood;
        public readonly Chunk Chunk;
        private RenderMesh<BlockVertex> _mesh;
        public bool RequiresRebuild;
        public Task<MeshBuilder<BlockVertex>>? RebuildTask;
        public readonly WorldRenderer WorldRenderer;

        public ChunkRenderer(Vector3i pos, Chunk chunk, WorldRenderer worldRenderer)
        {
            Pos = pos;
            Chunk = chunk;
            WorldRenderer = worldRenderer;
            Neighbourhood = new ChunkRendererNeighbourhood(pos);
            _mesh = new RenderMesh<BlockVertex>(VertexFormats.Block);
            RequiresRebuild = true;
        }

        public bool HasAnythingToRender()
        {
            return _mesh.IndexCount > 0;
        }

        public MeshBuilder<BlockVertex> Rebuild()
        {
            MeshBuilder<BlockVertex> mb = new(VertexFormats.Block);
            mb.MatrixStack.Push();
            for (int i = 0; i < Chunk.Size; ++i)
            {
                for (int j = 0; j < Chunk.Size; ++j)
                {
                    for (int k = 0; k < Chunk.Size; ++k)
                    {
                        Vector3i relativePos = new(i, j, k);
                        Vector3i absolutePos = Pos.BitShiftLeft(Chunk.SizeLog2) + relativePos;
                        BlockState state = Neighbourhood.GetBlockState(absolutePos);
                        if (state.Block != Blocks.Air)
                        {
                            mb.MatrixStack.Push();
                            mb.MatrixStack.Translate(relativePos);
                            WorldRenderer.BlockRenderer.RenderBlock(state, Neighbourhood, absolutePos, mb);
                            mb.MatrixStack.Pop();
                        }
                    }
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

        public void Dispose()
        {
            _mesh.DeleteGl();
            GC.SuppressFinalize(this);
        }
    }
}

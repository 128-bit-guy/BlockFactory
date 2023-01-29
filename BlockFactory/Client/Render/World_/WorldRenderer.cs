using OpenTK.Mathematics;
using BlockFactory.Block_;
using BlockFactory.Client.Render.Shader;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Side_;
using BlockFactory.Util.Math_;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;

namespace BlockFactory.Client.Render.World_
{
    [ExclusiveTo(Side.Client)]
    public class WorldRenderer : IDisposable
    {
        public readonly World World;
        public readonly Dictionary<Vector3i, ChunkRenderer> ChunkRenderers;
        public readonly PlayerEntity Player;
        //private RenderMesh<BlockVertex> _mesh = null!;
        public readonly BlockRenderer BlockRenderer;

        public WorldRenderer(World world, PlayerEntity player)
        {
            World = world;
            Player = player;
            ChunkRenderers = new Dictionary<Vector3i, ChunkRenderer>();
            player.ChunkBecameVisible += OnChunkReadyForUse;
            player.ChunkBecameInvisible += OnChunkNotReadyForUse;
            player.OnVisibleBlockChange += OnVisibleBlockChange;
            BlockRenderer = new BlockRenderer(this);
            //_mesh = new RenderMesh<BlockVertex>(VertexFormats.Block);
            //MeshBuilder<BlockVertex> mb = new MeshBuilder<BlockVertex>(VertexFormats.Block);
            //mb.Layer = Textures.Dirt;
            //foreach (var direction in DirectionUtils.GetValues())
            //{
            //    CubeRotation rotation;
            //    if (direction.GetAxis() == 1)
            //    {
            //        rotation = CubeRotation.GetFromTo(Direction.North, direction)[0];
            //    }
            //    else
            //    {
            //        rotation = CubeRotation.GetFromToKeeping(Direction.North, direction, Direction.Up);
            //    }
            //    float light = 1.0f - DirectionUtils.GetAxis(direction) * 0.1f;
            //    mb.MatrixStack.Push();
            //    mb.MatrixStack.Multiply((rotation).Matrix4);
            //    mb.BeginIndexSpace();
            //    mb.AddVertex((1f, 0f, 0f, light, light, light, 0f, 0f));
            //    mb.AddVertex((1f, 0f, 1f, light, light, light, 0f, 1f));
            //    mb.AddVertex((1f, 1f, 1f, light, light, light, 1f, 1f));
            //    mb.AddVertex((1f, 1f, 0f, light, light, light, 1f, 0f));
            //    mb.AddIndices(0, 2, 1, 0, 3, 2);
            //    mb.EndIndexSpace();
            //    mb.MatrixStack.Pop();
            //}
            //mb.Upload(_mesh);
            //mb.Reset();
        }

        private void OnChunkReadyForUse(Chunk chunk)
        {
            Vector3i pos = chunk.Pos;
            ChunkRenderer cr = new(pos, chunk, this);
            ChunkRenderers.Add(pos, cr);
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    for (int k = -1; k <= 1; ++k)
                    {
                        Vector3i oPos = pos + new Vector3i(i, j, k);
                        if (ChunkRenderers.TryGetValue(oPos, out ChunkRenderer? ocr))
                        {
                            ocr.Neighbourhood.OnChunkLoaded(chunk);
                            if (cr != ocr)
                            {
                                cr.Neighbourhood.OnChunkLoaded(ocr.Chunk);
                            }
                        }
                    }
                }
            }
        }

        private void OnChunkNotReadyForUse(Chunk chunk)
        {
            Vector3i pos = chunk.Pos;
            ChunkRenderers.Remove(pos, out ChunkRenderer? cr);
            for (int i = -1; i <= 1; ++i)
            {
                for (int j = -1; j <= 1; ++j)
                {
                    for (int k = -1; k <= 1; ++k)
                    {
                        Vector3i oPos = pos + new Vector3i(i, j, k);
                        if (ChunkRenderers.TryGetValue(oPos, out ChunkRenderer? ocr))
                        {
                            ocr.Neighbourhood.OnChunkUnloaded(pos);
                            if (cr != ocr)
                            {
                                cr!.Neighbourhood.OnChunkUnloaded(oPos);
                            }
                        }
                    }
                }
            }
        }

        public void UpdateAndRender()
        {
            Shaders.Block.Use();
            Textures.BlockArray.Bind();
            //Shaders.Block.SetColor((1f, 1f, 1f, 1f));
            Shaders.Block.SetModel(Matrix4.Identity);
            BlockFactoryClient.Instance.VpMatrices.Set(Shaders.Block);
            //_mesh.Bind();
            FrustumIntersectionHelper intersectionHelper = new FrustumIntersectionHelper(BlockFactoryClient.Instance.VpMatrices);
            int leftParallelRebuilds = 8;
            foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
            {
                bool hasRebuildTask = chunkRenderer.RebuildTask != null;
                if (hasRebuildTask && !chunkRenderer.RebuildTask!.IsCompleted) {
                    --leftParallelRebuilds;
                }
                Vector3i translation = (chunkRenderer.Pos - BlockFactoryClient.Instance.Player!.Pos.ChunkPos).BitShiftLeft(Chunk.SizeLog2);
                Box3 box = new Box3(new Vector3(0), new Vector3(Chunk.Size)).Add(translation.ToVector3());
                if (intersectionHelper.TestAab(box))
                {
                    if (chunkRenderer.Neighbourhood.LoadedChunkCnt == 27 && chunkRenderer.RequiresRebuild && !hasRebuildTask && leftParallelRebuilds > 0)
                    {
                        chunkRenderer.RequiresRebuild = false;
                        chunkRenderer.RebuildTask = Task.Run(chunkRenderer.Rebuild);
                        --leftParallelRebuilds;
                    }
                    if (chunkRenderer.RebuildTask != null && chunkRenderer.RebuildTask.IsCompletedSuccessfully) {
                        chunkRenderer.Upload(chunkRenderer.RebuildTask.Result);
                        chunkRenderer.RebuildTask = null;
                    }
                    Shaders.Block.SetModel(Matrix4.CreateTranslation(translation));
                    if (chunkRenderer.HasAnythingToRender())
                    {
                        chunkRenderer.Render();
                    }
                }
                //if (chunkRenderer.Neighbourhood.LoadedChunkCnt != 27)
                //{
                //if (chunkRenderer.Chunk.Data!.GetBlockState((0, 0, 0)).Block != Blocks.Stone && chunkRenderer.Neighbourhood.LoadedChunkCnt == 27)
                //{
                //    _mesh.Bind();
                //    GL.DrawElements(PrimitiveType.Triangles, _mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
                //}
                //}
            }
        }

        private void OnVisibleBlockChange(Chunk chunk, Vector3i pos, BlockState prevState, BlockState newState)
        {
            for (var i = -1; i <= 1; ++i)
            {
                for (var j = -1; j <= 1; ++j)
                {
                    for (var k = -1; k <= 1; ++k)
                    {
                        var blockPos = pos + new Vector3i(i, j, k);
                        var chunkPos = blockPos.BitShiftRight(Chunk.SizeLog2);
                        if (ChunkRenderers.TryGetValue(chunkPos, out var r))
                        {
                            r.RequiresRebuild = true;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            Player.ChunkBecameVisible -= OnChunkReadyForUse;
            Player.ChunkBecameInvisible -= OnChunkNotReadyForUse;
            Player.OnVisibleBlockChange -= OnVisibleBlockChange;
            foreach (ChunkRenderer chunkRenderer in ChunkRenderers.Values)
            {
                chunkRenderer.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}

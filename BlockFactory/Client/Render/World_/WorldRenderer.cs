using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client.Entity_;
using BlockFactory.Client.Render.Shader;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Side_;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockFactory.Client.Render.World_;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    //private RenderMesh<BlockVertex> _mesh = null!;
    public readonly BlockRenderer BlockRenderer;
    public readonly Dictionary<Vector3i, ChunkRenderer> ChunkRenderers;
    public readonly PlayerEntity Player;
    public readonly World World;
    public readonly WorldStreamMeshes StreamMeshes;


    public WorldRenderer(BlockFactoryClient client, World world, PlayerEntity player)
    {
        World = world;
        Player = player;
        ChunkRenderers = new Dictionary<Vector3i, ChunkRenderer>();
        player.ChunkBecameVisible += OnChunkReadyForUse;
        player.ChunkBecameInvisible += OnChunkNotReadyForUse;
        player.OnVisibleBlockChange += OnVisibleBlockChange;
        BlockRenderer = new BlockRenderer(this);
        StreamMeshes = new WorldStreamMeshes(client);
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

    public void Dispose()
    {
        Player.ChunkBecameVisible -= OnChunkReadyForUse;
        Player.ChunkBecameInvisible -= OnChunkNotReadyForUse;
        Player.OnVisibleBlockChange -= OnVisibleBlockChange;
        foreach (var chunkRenderer in ChunkRenderers.Values) chunkRenderer.Dispose();
        StreamMeshes.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnChunkReadyForUse(Chunk chunk)
    {
        var pos = chunk.Pos;
        ChunkRenderer cr = new(pos, chunk, this);
        ChunkRenderers.Add(pos, cr);
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var oPos = pos + new Vector3i(i, j, k);
            if (ChunkRenderers.TryGetValue(oPos, out var ocr))
            {
                ocr.Neighbourhood.OnChunkLoaded(chunk);
                if (cr != ocr) cr.Neighbourhood.OnChunkLoaded(ocr.Chunk);
            }
        }
    }

    private void OnChunkNotReadyForUse(Chunk chunk)
    {
        var pos = chunk.Pos;
        ChunkRenderers.Remove(pos, out var cr);
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var oPos = pos + new Vector3i(i, j, k);
            if (ChunkRenderers.TryGetValue(oPos, out var ocr))
            {
                ocr.Neighbourhood.OnChunkUnloaded(pos);
                if (cr != ocr) cr!.Neighbourhood.OnChunkUnloaded(oPos);
            }
        }
    }

    private int GetSortKey(ChunkRenderer cr)
    {
        return (cr.Pos - Player.Pos.ChunkPos).SquareLength();
    }

    private int Compare(ChunkRenderer a, ChunkRenderer b)
    {
        return GetSortKey(a) - GetSortKey(b);
    }

    public void UpdateAndRender()
    {
        Shaders.Block.Use();
        Textures.BlockArray.Bind();
        //Shaders.Block.SetColor((1f, 1f, 1f, 1f));
        Shaders.Block.SetModel(Matrix4.Identity);
        BlockFactoryClient.Instance.VpMatrices.Set(Shaders.Block);
        GL.Uniform3(Shaders.PlayerPosUniform, BlockFactoryClient.Instance.Player!.Pos.PosInChunk);
        //_mesh.Bind();
        var intersectionHelper = new FrustumIntersectionHelper(BlockFactoryClient.Instance.VpMatrices);
        var leftParallelRebuilds = 4;
        var stack = BlockFactoryClient.Instance.Matrices;
        stack.Push();
        var renderersSorted = new List<ChunkRenderer>();
        foreach (var chunkRenderer in ChunkRenderers.Values)
        {
            var hasRebuildTask = chunkRenderer.RebuildTask != null;
            if (hasRebuildTask && !chunkRenderer.RebuildTask!.IsCompleted) --leftParallelRebuilds;
            var translation =
                (chunkRenderer.Pos - BlockFactoryClient.Instance.Player!.Pos.ChunkPos).BitShiftLeft(Constants
                    .ChunkSizeLog2);
            var box = new Box3(new Vector3(0), new Vector3(Constants.ChunkSize)).Add(translation.ToVector3());
            if (intersectionHelper.TestAab(box))
            {
                renderersSorted.Add(chunkRenderer);
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
        renderersSorted.Sort(Compare);
        foreach (var chunkRenderer in renderersSorted)
        {
            var hasRebuildTask = chunkRenderer.RebuildTask != null;
            var translation =
                (chunkRenderer.Pos - BlockFactoryClient.Instance.Player!.Pos.ChunkPos).BitShiftLeft(Constants
                    .ChunkSizeLog2);
            stack.Push();
            stack.Translate(translation);
            if (chunkRenderer.Neighbourhood.LoadedChunkCnt == 27 && chunkRenderer.RequiresRebuild &&
                !hasRebuildTask && leftParallelRebuilds > 0)
            {
                chunkRenderer.RequiresRebuild = false;
                chunkRenderer.RebuildTask = Task.Run(chunkRenderer.Rebuild);
                --leftParallelRebuilds;
            }

            if (chunkRenderer.RebuildTask is { IsCompletedSuccessfully: true })
            {
                chunkRenderer.Upload(chunkRenderer.RebuildTask.Result);
                chunkRenderer.RebuildTask = null;
            }

            Shaders.Block.SetModel(stack);
            if (chunkRenderer.HasAnythingToRender()) chunkRenderer.Render();
            foreach (var entity in chunkRenderer.Chunk.Data.EntitiesInChunk.Values)
            {
                stack.Push();
                stack.Translate(entity.GetInterpolatedPos().PosInChunk);
                if (entity is ItemEntity item)
                {
                    stack.Push();
                    stack.RotateY((float)GLFW.GetTime() / 3);
                    stack.Scale(0.2f);
                    stack.Translate(new Vector3(-0.5f));
                    BlockFactoryClient.Instance.ItemRenderer!.RenderItemStack(item.Stack,
                        StreamMeshes.Block.Builder);
                    stack.Pop();
                }

                stack.Pop();
            }

            stack.Pop();
        }

        stack.Pop();
        
        StreamMeshes.FlushAll();
    }

    private void OnVisibleBlockChange(Chunk chunk, Vector3i pos, BlockState prevState, BlockState newState)
    {
        for (var i = -1; i <= 1; ++i)
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var blockPos = pos + new Vector3i(i, j, k);
            var chunkPos = blockPos.BitShiftRight(Constants.ChunkSizeLog2);
            if (ChunkRenderers.TryGetValue(chunkPos, out var r)) r.RequiresRebuild = true;
        }
    }
}
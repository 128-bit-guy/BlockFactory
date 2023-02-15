using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client.Render.Shader;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Side_;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render.World_;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    //private RenderMesh<BlockVertex> _mesh = null!;
    public readonly BlockRenderer BlockRenderer;
    public readonly Dictionary<Vector3i, ChunkRenderer> ChunkRenderers;
    public readonly PlayerEntity Player;
    public readonly World World;

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

    public void Dispose()
    {
        Player.ChunkBecameVisible -= OnChunkReadyForUse;
        Player.ChunkBecameInvisible -= OnChunkNotReadyForUse;
        Player.OnVisibleBlockChange -= OnVisibleBlockChange;
        foreach (var chunkRenderer in ChunkRenderers.Values) chunkRenderer.Dispose();
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
        //_mesh.Bind();
        var intersectionHelper = new FrustumIntersectionHelper(BlockFactoryClient.Instance.VpMatrices);
        var leftParallelRebuilds = 4;
        var renderersSorted = new List<ChunkRenderer>();
        renderersSorted.AddRange(ChunkRenderers.Values);
        renderersSorted.Sort(Compare);
        foreach (var chunkRenderer in renderersSorted)
        {
            var hasRebuildTask = chunkRenderer.RebuildTask != null;
            if (hasRebuildTask && !chunkRenderer.RebuildTask!.IsCompleted) --leftParallelRebuilds;
            var translation =
                (chunkRenderer.Pos - BlockFactoryClient.Instance.Player!.Pos.ChunkPos).BitShiftLeft(Constants.ChunkSizeLog2);
            var box = new Box3(new Vector3(0), new Vector3(Constants.ChunkSize)).Add(translation.ToVector3());
            if (intersectionHelper.TestAab(box))
            {
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

                Shaders.Block.SetModel(Matrix4.CreateTranslation(translation));
                if (chunkRenderer.HasAnythingToRender()) chunkRenderer.Render();
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
        for (var j = -1; j <= 1; ++j)
        for (var k = -1; k <= 1; ++k)
        {
            var blockPos = pos + new Vector3i(i, j, k);
            var chunkPos = blockPos.BitShiftRight(Constants.ChunkSizeLog2);
            if (ChunkRenderers.TryGetValue(chunkPos, out var r)) r.RequiresRebuild = true;
        }
    }
}
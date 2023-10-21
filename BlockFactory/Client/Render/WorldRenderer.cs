using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Math_;
using BlockFactory.World_;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    private readonly Stack<BlockMeshBuilder> _blockMeshBuilders = new();
    private readonly List<ChunkRenderer> _fadingOutRenderers = new();
    private readonly ChunkRenderer?[] _renderers = new ChunkRenderer?[1 << (3 * PlayerChunkLoading.CkdPowerOf2)];
    public readonly World World;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & PlayerChunkLoading.CkdMask) |
               (((pos.Y & PlayerChunkLoading.CkdMask) |
                 ((pos.Z & PlayerChunkLoading.CkdMask) << PlayerChunkLoading.CkdPowerOf2)) <<
                PlayerChunkLoading.CkdPowerOf2);
    }

    public WorldRenderer(World world)
    {
        World = world;
        world.ChunkStatusManager.ChunkReadyForTick += OnChunkReadyForTick;
        world.ChunkStatusManager.ChunkNotReadyForTick += OnChunkNotReadyForTick;
        for (var i = 0; i < 4; ++i) _blockMeshBuilders.Push(new BlockMeshBuilder());
    }

    public int RenderedChunks => _renderers.Count(c => c != null);
    public int FadingOutChunks => _fadingOutRenderers.Count;

    public void Dispose()
    {
        World.ChunkStatusManager.ChunkReadyForTick -= OnChunkReadyForTick;
        World.ChunkStatusManager.ChunkNotReadyForTick -= OnChunkNotReadyForTick;
        for (int i = 0; i < _renderers.Length; ++i)
        {
            if (_renderers[i] == null) continue;
            _renderers[i]!.Dispose();
            _renderers[i] = null;
        }
    }

    private void OnChunkReadyForTick(Chunk c)
    {
        var cr = new ChunkRenderer(c);
        _renderers[GetArrIndex(cr.Chunk.Position)] = cr;
    }

    private void OnChunkNotReadyForTick(Chunk c)
    {
        var cr = _renderers[GetArrIndex(c.Position)]!;
        _renderers[GetArrIndex(c.Position)] = null;
        cr!.Valid = false;
        if (cr.RebuildTask != null)
        {
            try
            {
                cr.RebuildTask.Wait();
            }
            catch (Exception ex)
            {
                //
            }

            cr.MeshBuilder!.MeshBuilder.Reset();
            _blockMeshBuilders.Push(cr.MeshBuilder);
            cr.RebuildTask = null;
            cr.MeshBuilder = null;
        }

        if (cr.Mesh.IndexCount > 0)
        {
            cr.Unloading = true;
            _fadingOutRenderers.Add(cr);
        }
        else
        {
            cr.Dispose();
        }
    }

    public void UpdateAndRender(double deltaTime)
    {
        Textures.Blocks.Bind();
        Shaders.Block.Use();
        Shaders.Block.SetSkyColor(BfRendering.SkyColor);
        foreach (var delta in PlayerChunkLoading.ChunkDeltas)
        {
            var pos = BlockFactoryClient.Player.GetChunkPos() + delta;
            var renderer = _renderers[GetArrIndex(pos)];
            if(renderer == null) continue;
            if (renderer.RequiresRebuild && renderer.RebuildTask == null && _blockMeshBuilders.Count > 0)
            {
                var bmb = _blockMeshBuilders.Pop();
                renderer.StartRebuildTask(bmb);
                renderer.RequiresRebuild = false;
            }

            if (renderer.RebuildTask is { IsCompleted: true })
            {
                if (renderer.RebuildTask.IsCompletedSuccessfully)
                    renderer.MeshBuilder!.MeshBuilder.Upload(renderer.Mesh);

                renderer.MeshBuilder!.MeshBuilder.Reset();
                _blockMeshBuilders.Push(renderer.MeshBuilder);
                renderer.RebuildTask = null;
                renderer.MeshBuilder = null;
                renderer.Initialized = true;
            }

            UpdateAndRenderChunk(renderer, deltaTime);
        }

        foreach (var renderer in _fadingOutRenderers) UpdateAndRenderChunk(renderer, deltaTime);

        _fadingOutRenderers.RemoveAll(renderer => renderer.LoadProgress <= 0.01f);

        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }
    
    private Vector3D<float> GetChunkTranslation(ChunkRenderer renderer) {
        return (renderer.Chunk.Position
            .ShiftLeft(Constants.ChunkSizeLog2).As<double>() - BlockFactoryClient.Player.Pos).As<float>();
    }

    private unsafe void UpdateAndRenderChunk(ChunkRenderer renderer, double deltaTime)
    {
        renderer.Update(deltaTime);
        if (renderer.Mesh.IndexCount == 0) return;
        Shaders.Block.SetModel(Matrix4X4.CreateTranslation(GetChunkTranslation(renderer)));
        Shaders.Block.SetLoadProgress(renderer.LoadProgress);
        renderer.Mesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, renderer.Mesh.IndexCount, DrawElementsType.UnsignedInt,
            null);
    }
}
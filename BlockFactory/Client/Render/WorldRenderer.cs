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
    private readonly Dictionary<Vector3D<int>, ChunkRenderer> _renderers = new();
    public readonly World World;

    public WorldRenderer(World world)
    {
        World = world;
        world.ChunkStatusManager.ChunkReadyForTick += OnChunkReadyForTick;
        world.ChunkStatusManager.ChunkNotReadyForTick += OnChunkNotReadyForTick;
        for (var i = 0; i < 4; ++i) _blockMeshBuilders.Push(new BlockMeshBuilder());
    }

    public int RenderedChunks => _renderers.Count;
    public int FadingOutChunks => _fadingOutRenderers.Count;

    public void Dispose()
    {
        foreach (var (pos, renderer) in _renderers) renderer.Dispose();

        _renderers.Clear();
        World.ChunkStatusManager.ChunkReadyForTick -= OnChunkReadyForTick;
        World.ChunkStatusManager.ChunkNotReadyForTick -= OnChunkNotReadyForTick;
    }

    private void OnChunkReadyForTick(Chunk c)
    {
        var cr = new ChunkRenderer(c);
        _renderers.Add(c.Position, cr);
    }

    private void OnChunkNotReadyForTick(Chunk c)
    {
        _renderers.Remove(c.Position, out var cr);
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
        foreach (var (pos, renderer) in _renderers)
        {
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

    private unsafe void UpdateAndRenderChunk(ChunkRenderer renderer, double deltaTime)
    {
        renderer.Update(deltaTime);
        if (renderer.Mesh.IndexCount == 0) return;
        Shaders.Block.SetModel(Matrix4X4.CreateTranslation(renderer.Chunk.Position
            .ShiftLeft(Constants.ChunkSizeLog2).As<float>()));
        Shaders.Block.SetLoadProgress(renderer.LoadProgress);
        renderer.Mesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, renderer.Mesh.IndexCount, DrawElementsType.UnsignedInt,
            null);
    }
}
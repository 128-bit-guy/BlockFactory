using BlockFactory.Base;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Math_;
using BlockFactory.World_;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    public readonly World World;
    private readonly Dictionary<Vector3D<int>, ChunkRenderer> _renderers = new();
    private readonly Stack<BlockMeshBuilder> _blockMeshBuilders = new();

    public WorldRenderer(World world)
    {
        World = world;
        world.ChunkStatusManager.ChunkReadyForTick += OnChunkReadyForTick;
        world.ChunkStatusManager.ChunkNotReadyForTick += OnChunkNotReadyForTick;
        for (var i = 0; i < 4; ++i)
        {
            _blockMeshBuilders.Push(new BlockMeshBuilder());
        }
    }

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
        cr!.Dispose();
    }

    public unsafe void UpdateAndRender()
    {
        Textures.Blocks.Bind();
        Shaders.Block.Use();
        foreach (var (pos, renderer) in _renderers)
        {
            if (renderer.RequiresUpdate && renderer.RebuildTask == null &&  _blockMeshBuilders.Count > 0)
            {
                var bmb = _blockMeshBuilders.Pop();
                renderer.StartRebuildTask(bmb);
                renderer.RequiresUpdate = false;
            }

            if (renderer.RebuildTask is { IsCompleted: true })
            {
                if (renderer.RebuildTask.IsCompletedSuccessfully)
                {
                    renderer.MeshBuilder!.MeshBuilder.Upload(renderer.Mesh);
                }

                renderer.MeshBuilder!.MeshBuilder.Reset();
                _blockMeshBuilders.Push(renderer.MeshBuilder);
                renderer.RebuildTask = null;
                renderer.MeshBuilder = null;
            }

            if (renderer.Mesh.IndexCount == 0) continue;
            Shaders.Block.SetModel(Matrix4X4.CreateTranslation(pos.ShiftLeft(Constants.ChunkSizeLog2).As<float>()));
            renderer.Mesh.Bind();
            BfRendering.Gl.DrawElements(PrimitiveType.Triangles, renderer.Mesh.IndexCount, DrawElementsType.UnsignedInt,
                null);
        }

        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}
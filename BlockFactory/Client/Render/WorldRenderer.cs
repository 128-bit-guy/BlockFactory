using BlockFactory.Base;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.World_;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    private Dictionary<Vector3D<int>, ChunkRenderer> _renderers = new();

    public readonly World World;

    public WorldRenderer(World world)
    {
        World = world;
        world.ChunkReadyForTick += OnChunkReadyForTick;
        world.ChunkNotReadyForTick += OnChunkNotReadyForTick;
    }

    private void OnChunkReadyForTick(Chunk c)
    {
        var cr = new ChunkRenderer(c);
        // cr.RebuildChunkMesh();
        _renderers.Add(c.Position, cr);
    }

    private void OnChunkNotReadyForTick(Chunk c)
    {
        _renderers.Remove(c.Position, out var cr);
        cr!.Dispose();
    }
    public void Dispose()
    {
        foreach (var (pos, renderer) in _renderers)
        {
            renderer.Dispose();
        }

        _renderers.Clear();
        World.ChunkReadyForTick -= OnChunkReadyForTick;
        World.ChunkNotReadyForTick -= OnChunkNotReadyForTick;
    }

    public unsafe void UpdateAndRender()
    {
        Textures.Blocks.Bind();
        Shaders.Block.Use();
        var leftUpdates = 2;
        foreach (var (pos, renderer) in _renderers)
        {
            if (renderer.RequiresUpdate && leftUpdates > 0)
            {
                renderer.RebuildChunkMesh();
                renderer.RequiresUpdate = false;
                --leftUpdates;
            }
            if (renderer.Mesh.IndexCount == 0) continue;
            Shaders.Block.SetModel(Matrix4X4.CreateTranslation(pos.ShiftLeft(Constants.ChunkSizeLog2).As<float>()));
            renderer.Mesh.Bind();
            BfRendering.Gl.DrawElements(PrimitiveType.Triangles, renderer.Mesh.IndexCount, DrawElementsType.UnsignedInt, null);
        }
        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}
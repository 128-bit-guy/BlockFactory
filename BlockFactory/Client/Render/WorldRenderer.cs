using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Client.Render.Block_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.Content.Block_;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Content.Item_;
using BlockFactory.Utils;
using BlockFactory.World_;
using BlockFactory.World_.ChunkLoading;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class WorldRenderer : IDisposable
{
    private readonly Stack<BlockMeshBuilder> _blockMeshBuilders = new();
    private readonly List<ChunkRenderer> _fadingOutRenderers = new();
    private readonly ChunkRenderer?[] _renderers = new ChunkRenderer?[1 << (3 * PlayerChunkLoading.CkdPowerOf2)];

    private readonly List<ChunkRenderer> _transparentRenderers = new();

    private readonly DynamicMesh _dynamicMesh;

    public readonly PlayerEntity Player;
    private Vector3D<double> _playerSmoothPos;

    public WorldRenderer(PlayerEntity player)
    {
        Player = player;
        player.ChunkBecameVisible += OnChunkReadyForTick;
        player.ChunkBecameInvisible += OnChunkNotReadyForTick;
        for (var i = 0; i < 4; ++i) _blockMeshBuilders.Push(new BlockMeshBuilder());
        _dynamicMesh = new DynamicMesh();
    }

    public int RenderedChunks => _renderers.Count(c => c != null);
    public int FadingOutChunks => _fadingOutRenderers.Count;

    public void Dispose()
    {
        Player.ChunkBecameVisible -= OnChunkReadyForTick;
        Player.ChunkBecameInvisible -= OnChunkNotReadyForTick;
        for (var i = 0; i < _renderers.Length; ++i)
        {
            if (_renderers[i] == null) continue;
            _renderers[i]!.Dispose();
            _renderers[i] = null;
        }
        _dynamicMesh.Dispose();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & PlayerChunkLoading.CkdMask) |
               (((pos.Y & PlayerChunkLoading.CkdMask) |
                 ((pos.Z & PlayerChunkLoading.CkdMask) << PlayerChunkLoading.CkdPowerOf2)) <<
                PlayerChunkLoading.CkdPowerOf2);
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

            cr.MeshBuilder!.Reset();
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
        _playerSmoothPos = BlockFactoryClient.Player!.GetSmoothPos();
        var intersectionHelper = BfRendering.CreateIntersectionHelper();
        Textures.Blocks.Bind();
        Shaders.Block.Use();
        Shaders.Block.SetSkyColor(BfRendering.SkyColor);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Blocks.SpriteBoxesBuffer.Bind(2);
        var transparentRenderers = _transparentRenderers;
        var maxRebuilds = _blockMeshBuilders.Count;
        foreach (var delta in WorldRendering.ChunkDeltas)
        {
            var pos = BlockFactoryClient.Player.GetChunkPos() + delta;
            var renderer = _renderers[GetArrIndex(pos)];
            if (renderer == null) continue;

            if (renderer.RebuildTask is { IsCompleted: true })
            {
                if (renderer.RebuildTask.IsCompletedSuccessfully)
                {
                    renderer.MeshBuilder!.MeshBuilder.Upload(renderer.Mesh);
                    renderer.TransparentStart = renderer.MeshBuilder.TransparentStart;
                }

                renderer.MeshBuilder!.Reset();
                _blockMeshBuilders.Push(renderer.MeshBuilder);
                renderer.RebuildTask = null;
                renderer.MeshBuilder = null;
                renderer.Initialized = true;
            }

            var translation = GetChunkTranslation(renderer);

            var b = new Box3D<float>(translation, translation + new Vector3D<float>(Constants.ChunkSize));

            if (!intersectionHelper.TestAab(b)) continue;

            if (renderer.RequiresRebuild && renderer.RebuildTask == null && _blockMeshBuilders.Count > 0 &&
                renderer.Chunk.ChunkStatusInfo.ReadyForTick && maxRebuilds > 0)
            {
                --maxRebuilds;
                var bmb = _blockMeshBuilders.Pop();
                renderer.StartRebuildTask(bmb);
                renderer.RequiresRebuild = false;
            }

            renderer.Update(deltaTime);
            RenderChunk(renderer, false);
            foreach (var (_, entity) in renderer.Chunk.Data!.Entities)
            {
                if (entity is not ItemEntity item) continue;
                _dynamicMesh.Matrices.Push();
                var entitySmoothPos = entity.GetSmoothPos();
                _dynamicMesh.Matrices.Translate((entitySmoothPos - _playerSmoothPos).As<float>());
                _dynamicMesh.Matrices.Scale(0.3f);
                var brightness =
                    (float)LightInterpolation.GetInterpolatedBrightness(renderer.Chunk.Neighbourhood, entitySmoothPos);
                var color = new Vector4D<float>(brightness, brightness, brightness, 1);
                _dynamicMesh.SetColor(color);
                if (item.Stack.ItemInstance.Item is BlockItem)
                {
                    _dynamicMesh.Matrices.RotateY((float)((BlockFactoryClient.Window.Time + entity.HeadRotation.X) % (2 * Math.PI)));
                }
                else
                {
                    _dynamicMesh.Matrices.RotateY(Player.HeadRotation.X + MathF.PI);
                    _dynamicMesh.Matrices.RotateX(Player.HeadRotation.Y);
                }

                ItemRenderer.RenderItemStack(item.Stack, _dynamicMesh);
                _dynamicMesh.SetColor(Vector4D<float>.One);
                _dynamicMesh.Matrices.Pop();
            }
            if (renderer.TransparentStart != renderer.Mesh.IndexCount) transparentRenderers.Add(renderer);
        }
        
        BfRendering.Matrices.Push();
        _dynamicMesh.Render();
        BfRendering.Matrices.Pop();
        
        Textures.Blocks.Bind();
        Shaders.Block.Use();
        Shaders.Block.SetSkyColor(BfRendering.SkyColor);
        Shaders.Block.SetSpriteBoxesBinding(2);
        Textures.Blocks.SpriteBoxesBuffer.Bind(2);

        foreach (var renderer in _fadingOutRenderers)
        {
            renderer.Update(deltaTime);
            RenderChunk(renderer, false);
        }

        _fadingOutRenderers.RemoveAll(renderer => renderer.LoadProgress <= 0.01f);

        BfRendering.Gl.Enable(EnableCap.Blend);
        BfRendering.Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        foreach (var renderer in transparentRenderers) RenderChunk(renderer, true);
        transparentRenderers.Clear();
        BfRendering.Gl.Disable(EnableCap.Blend);

        BfRendering.Gl.BindVertexArray(0);
        BfRendering.Gl.UseProgram(0);
        BfRendering.Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    private Vector3D<float> GetChunkTranslation(ChunkRenderer renderer)
    {
        return (renderer.Chunk.Position
            .ShiftLeft(Constants.ChunkSizeLog2).As<double>() - _playerSmoothPos).As<float>();
    }

    private unsafe void RenderChunk(ChunkRenderer renderer, bool transparent)
    {
        uint begin, end;
        if (transparent)
        {
            begin = renderer.TransparentStart;
            end = renderer.Mesh.IndexCount;
        }
        else
        {
            begin = 0;
            end = renderer.TransparentStart;
        }

        if (begin == end) return;
        BfRendering.Matrices.Push();
        BfRendering.Matrices.Translate(GetChunkTranslation(renderer));
        Shaders.Block.SetModel(BfRendering.Matrices);
        Shaders.Block.SetLoadProgress(renderer.LoadProgress);
        renderer.Mesh.Bind();
        BfRendering.Gl.DrawElements(PrimitiveType.Triangles, end - begin,
            DrawElementsType.UnsignedInt, (void*)(begin * sizeof(uint)));
        BfRendering.Matrices.Pop();
    }
}
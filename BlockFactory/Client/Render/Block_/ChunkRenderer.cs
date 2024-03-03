using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.CubeMath;
using BlockFactory.Math_;
using BlockFactory.World_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class ChunkRenderer : IDisposable
{
    private static readonly uint[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
    private static readonly uint[] QuadIndices2 = { 0, 1, 3, 1, 2, 3 };
    private static readonly bool[] DifferentTriangles = new bool[1 << 16];
    private readonly float[] _vertexLight = new float[4];
    public readonly Chunk Chunk;
    public readonly RenderMesh Mesh;
    public bool Initialized = false;
    public float LoadProgress;
    public BlockMeshBuilder? MeshBuilder;
    public Task? RebuildTask;
    public bool RequiresRebuild = true;
    public uint TransparentStart = 0;
    public bool Unloading;
    public bool Valid = true;

    static ChunkRenderer()
    {
        int[] order = { 0, 1, 3, 2 };
        var differentTriangles = new bool[1 << 4];
        for (var i = 0; i < 4; ++i)
        {
            differentTriangles[1 << order[i]] = (i & 1) == 0;
            differentTriangles[15 & ~(1 << order[i])] = (i & 1) == 0;
        }

        for (var mask = 0; mask < 1 << 16; ++mask)
        {
            var lights = new int[4];
            for (var i = 0; i < 4; ++i) lights[i] = (mask >> (i << 2)) & 15;

            var mi = lights.Min();
            var oMask = 0;
            for (var i = 0; i < 4; ++i)
                if (lights[i] == mi)
                    oMask |= 1 << i;

            DifferentTriangles[mask] = differentTriangles[oMask];
        }
    }

    public ChunkRenderer(Chunk chunk)
    {
        Chunk = chunk;
        Mesh = new RenderMesh();
        chunk.BlockUpdate += OnBlockUpdate;
        chunk.LightUpdate += OnBlockUpdate;
    }

    public void Dispose()
    {
        Chunk.BlockUpdate -= OnBlockUpdate;
        Chunk.LightUpdate -= OnBlockUpdate;
        Mesh.Dispose();
    }

    public void BuildChunkMesh(BlockMeshBuilder bmb)
    {
        var builder = bmb.MeshBuilder;
        var transformer = bmb.UvTransformer;
        var neighbourhood = Chunk.Neighbourhood;
        Span<byte> lightVal = stackalloc byte[4];
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            if (!Valid) return;
            var absPos = Chunk.Position.ShiftLeft(Constants.ChunkSizeLog2)
                         + new Vector3D<int>(i, j, k);
            var block = neighbourhood.GetBlockObj(absPos);
            if (block == Blocks.Air) continue;
            if (block == Blocks.Water) continue;
            builder.Matrices.Push();
            builder.Matrices.Translate(i, j, k);
            foreach (var face in CubeFaceUtils.Values())
            {
                transformer.Sprite = block.GetTexture(face);
                var oPos = absPos + face.GetDelta();
                if (neighbourhood.GetBlockObj(oPos).BlockRendering(face.GetOpposite())) continue;
                var s = face.GetAxis() == 1
                    ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                    : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;

                for (var u = 0; u < 2; ++u)
                for (var v = 0; v < 2; ++v)
                {
                    byte cLight = 0;
                    var ao = false;
                    for (var dx = -1; dx < 1; ++dx)
                    for (var dy = -1; dy < 1; ++dy)
                    {
                        var oPos2Rel = new Vector3D<int>(u + dx, v + dy, 1);
                        var oPos2Abs = absPos + oPos2Rel * s;
                        cLight = Math.Max(cLight, neighbourhood.GetLight(oPos2Abs, LightChannel.Block));
                        cLight = Math.Max(cLight, neighbourhood.GetLight(oPos2Abs, LightChannel.Sky));
                        if (neighbourhood.GetBlockObj(oPos2Abs).HasAo()) ao = true;
                    }

                    if (ao) cLight -= Math.Min(cLight, (byte)3);
                    lightVal[u | (v << 1)] = cLight;
                }

                var dtMask = 0;
                for (var l = 0; l < 4; ++l)
                {
                    _vertexLight[l] = (float)lightVal[l] / 15;
                    dtMask |= lightVal[l] << (l << 2);
                }

                builder.Matrices.Push();
                builder.Matrices.Multiply(s.AroundCenterMatrix4);
                builder.NewPolygon().Indices(DifferentTriangles[dtMask] ? QuadIndices2 : QuadIndices)
                    .Vertex(new BlockVertex(0, 0, 1, _vertexLight[0], _vertexLight[0], _vertexLight[0], 1, 0, 0))
                    .Vertex(new BlockVertex(1, 0, 1, _vertexLight[1], _vertexLight[1], _vertexLight[1], 1, 1, 0))
                    .Vertex(new BlockVertex(1, 1, 1, _vertexLight[3], _vertexLight[3], _vertexLight[3], 1, 1, 1))
                    .Vertex(new BlockVertex(0, 1, 1, _vertexLight[2], _vertexLight[2], _vertexLight[2], 1, 0, 1));
                builder.Matrices.Pop();
            }

            builder.Matrices.Pop();
        }

        bmb.TransparentStart = (uint)builder.IndexCount;
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            if (!Valid) return;
            var absPos = Chunk.Position.ShiftLeft(Constants.ChunkSizeLog2)
                         + new Vector3D<int>(i, j, k);
            var block = neighbourhood.GetBlockObj(absPos);
            if (block != Blocks.Water) continue;
            builder.Matrices.Push();
            builder.Matrices.Translate(i, j, k);
            foreach (var face in CubeFaceUtils.Values())
            {
                transformer.Sprite = block.GetTexture(face);
                var oPos = absPos + face.GetDelta();
                var neighbour = neighbourhood.GetBlockObj(oPos);
                if (neighbour.BlockRendering(face.GetOpposite())) continue;
                if (neighbour == block) continue;
                var s = face.GetAxis() == 1
                    ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                    : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;

                for (var u = 0; u < 2; ++u)
                for (var v = 0; v < 2; ++v)
                {
                    byte cLight = 0;
                    var ao = false;
                    for (var dx = -1; dx < 1; ++dx)
                    for (var dy = -1; dy < 1; ++dy)
                    {
                        var oPos2Rel = new Vector3D<int>(u + dx, v + dy, 1);
                        var oPos2Abs = absPos + oPos2Rel * s;
                        cLight = Math.Max(cLight, neighbourhood.GetLight(oPos2Abs, LightChannel.Block));
                        cLight = Math.Max(cLight, neighbourhood.GetLight(oPos2Abs, LightChannel.Sky));
                        if (neighbourhood.GetBlockObj(oPos2Abs).HasAo()) ao = true;
                    }

                    if (ao) cLight -= Math.Min(cLight, (byte)3);
                    lightVal[u | (v << 1)] = cLight;
                }

                var dtMask = 0;
                for (var l = 0; l < 4; ++l)
                {
                    _vertexLight[l] = (float)lightVal[l] / 15;
                    dtMask |= lightVal[l] << (l << 2);
                }

                builder.Matrices.Push();
                builder.Matrices.Multiply(s.AroundCenterMatrix4);
                builder.NewPolygon().Indices(DifferentTriangles[dtMask] ? QuadIndices2 : QuadIndices)
                    .Vertex(new BlockVertex(0, 0, 1, _vertexLight[0], _vertexLight[0], _vertexLight[0], 1, 0, 0))
                    .Vertex(new BlockVertex(1, 0, 1, _vertexLight[1], _vertexLight[1], _vertexLight[1], 1, 1, 0))
                    .Vertex(new BlockVertex(1, 1, 1, _vertexLight[3], _vertexLight[3], _vertexLight[3], 1, 1, 1))
                    .Vertex(new BlockVertex(0, 1, 1, _vertexLight[2], _vertexLight[2], _vertexLight[2], 1, 0, 1));
                builder.Matrices.Pop();
            }

            builder.Matrices.Pop();
        }
    }

    public void StartRebuildTask(BlockMeshBuilder bmb)
    {
        MeshBuilder = bmb;
        RebuildTask = Task.Run(() => BuildChunkMesh(bmb));
    }

    private void OnBlockUpdate(Vector3D<int> pos)
    {
        RequiresRebuild = true;
    }

    public void Update(double deltaTime)
    {
        if (Unloading)
            LoadProgress -= (float)deltaTime;
        else if (Initialized) LoadProgress += (float)deltaTime;

        LoadProgress = Math.Clamp(LoadProgress, 0, 1);
    }
}
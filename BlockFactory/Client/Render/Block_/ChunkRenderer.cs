using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.CubeMath;
using BlockFactory.Math_;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class ChunkRenderer : IDisposable
{
    private static readonly uint[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
    private static readonly uint[] QuadIndices2 = { 0, 1, 3, 1, 2, 3 };
    private static readonly bool[] DifferentTriangles = new bool[1 << 4];
    private readonly float[] _vertexLight = new float[4];
    public readonly Chunk Chunk;
    public readonly RenderMesh Mesh;
    public bool RequiresUpdate = true;
    public Task? RebuildTask;
    public BlockMeshBuilder? MeshBuilder;
    public bool Valid = true;

    static ChunkRenderer()
    {
        int[] order = { 0, 1, 3, 2 };
        for (var i = 0; i < 4; ++i)
        {
            DifferentTriangles[1 << order[i]] = (i & 1) == 0;
            DifferentTriangles[15 & ~(1 << order[i])] = (i & 1) == 0;
        }
    }

    public ChunkRenderer(Chunk chunk)
    {
        Chunk = chunk;
        Mesh = new RenderMesh();
        chunk.BlockUpdate += OnBlockUpdate;
    }

    public void Dispose()
    {
        Chunk.BlockUpdate -= OnBlockUpdate;
        Mesh.Dispose();
    }

    public void BuildChunkMesh(BlockMeshBuilder bmb)
    {
        var builder = bmb.MeshBuilder;
        var transformer = bmb.UvTransformer;
        var neighbourhood = Chunk.Neighbourhood;
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            if(!Valid) return;
            var absPos = Chunk.Position.ShiftLeft(Constants.ChunkSizeLog2)
                         + new Vector3D<int>(i, j, k);
            var block = neighbourhood.GetBlock(absPos);
            if (block == 0) continue;
            builder.Matrices.Push();
            builder.Matrices.Translate(i, j, k);
            transformer.Sprite = block - 1;
            foreach (var face in CubeFaceUtils.Values())
            {
                var oPos = absPos + face.GetDelta();
                if (neighbourhood.GetBlock(oPos) != 0) continue;
                var light = 1;
                var s = face.GetAxis() == 1
                    ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                    : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;

                var aoMask = 0;
                for (var u = 0; u < 2; ++u)
                for (var v = 0; v < 2; ++v)
                for (var dx = -1; dx < 1; ++dx)
                for (var dy = -1; dy < 1; ++dy)
                {
                    var oPos2Rel = new Vector3D<int>(u + dx, v + dy, 1);
                    var oPos2Abs = absPos + oPos2Rel * s;
                    if (neighbourhood.GetBlock(oPos2Abs) != 0) aoMask |= 1 << (u | (v << 1));
                }

                for (var l = 0; l < 4; ++l) _vertexLight[l] = light - ((aoMask & (1 << l)) != 0 ? 0.2f : 0f);
                builder.Matrices.Push();
                builder.Matrices.Multiply(s.AroundCenterMatrix4);
                builder.NewPolygon().Indices(DifferentTriangles[aoMask] ? QuadIndices2 : QuadIndices)
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
        RequiresUpdate = true;
    }
}
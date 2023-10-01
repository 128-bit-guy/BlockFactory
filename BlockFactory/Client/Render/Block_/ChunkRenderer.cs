using BlockFactory.Base;
using BlockFactory.Client.Render.Mesh_;
using BlockFactory.Client.Render.Texture_;
using BlockFactory.CubeMath;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Block_;

[ExclusiveTo(Side.Client)]
public class ChunkRenderer
{
    private static readonly uint[] QuadIndices = { 0, 1, 2, 0, 2, 3 };
    public readonly Chunk Chunk;

    public ChunkRenderer(Chunk chunk)
    {
        Chunk = chunk;
    }

    public void BuildChunkMesh(MeshBuilder<BlockVertex> builder, TextureAtlasUvTransformer transformer)
    {
        var neighbourhood = Chunk.Neighbourhood;
        for (var i = 0; i < Constants.ChunkSize; ++i)
        {
            for (var j = 0; j < Constants.ChunkSize; ++j)
            {
                for (var k = 0; k < Constants.ChunkSize; ++k)
                {
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
                        var light = (float)(face.GetAxis() + 8) / 10;
                        builder.Matrices.Push();
                        var s = face.GetAxis() == 1
                            ? CubeSymmetry.GetFromTo(CubeFace.Front, face, true)[0]
                            : CubeSymmetry.GetFromToKeepingRotation(CubeFace.Front, face, CubeFace.Top)!;
                        builder.Matrices.Multiply(s.AroundCenterMatrix4);
                        builder.NewPolygon().Indices(QuadIndices)
                            .Vertex(new BlockVertex(0, 0, 1, light, light, light, 1, 0, 0))
                            .Vertex(new BlockVertex(1, 0, 1, light, light, light, 1, 1, 0))
                            .Vertex(new BlockVertex(1, 1, 1, light, light, light, 1, 1, 1))
                            .Vertex(new BlockVertex(0, 1, 1, light, light, light, 1, 0, 1));
                        builder.Matrices.Pop();
                    }
                    builder.Matrices.Pop();
                }
            }
        }
    }
}
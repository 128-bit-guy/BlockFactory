using OpenTK.Mathematics;

namespace BlockFactory.CubeMath;

public class CubeEdge
{
    public static readonly CubeEdge[] Edges;
    public readonly byte Ordinal;
    public readonly CubeVertex[] Vertices;
    private static readonly CubeEdge?[,] FromVerticesArr;
    public CubeEdge Opposite { get; private set; }
    public Vector3 Center { get; private set; }

    static CubeEdge()
    {
        #region Edges
        
        List<CubeEdge> edgesList = new List<CubeEdge>();
        foreach (var vertex in CubeVertex.Vertices)
        {
            var neighbours = CubeFaceUtils.GetValues()
                .Select(f => f.GetOffset())
                .Select(v => vertex.Pos + v)
                .Select(CubeVertex.FromVector)
                .Where(v => v != null)
                .Where(v => v!.Ordinal > vertex.Ordinal);
            foreach (var neighbour in neighbours)
            {
                var nEdge = new CubeEdge((byte)edgesList.Count, vertex, neighbour!);
                edgesList.Add(nEdge);
            }
        }

        Edges = edgesList.ToArray();

        #endregion

        #region FromVertices

        FromVerticesArr = new CubeEdge?[CubeVertex.Vertices.Length, CubeVertex.Vertices.Length];
        foreach (var edge in Edges)
        {
            FromVerticesArr[edge.Vertices[0].Ordinal, edge.Vertices[1].Ordinal] =
                FromVerticesArr[edge.Vertices[1].Ordinal, edge.Vertices[0].Ordinal] = edge;
        }

        #endregion

        #region Centers

        foreach (var edge in Edges)
        {
            edge.Center = 1 / 2f * ((Vector3)edge.Vertices[0].Pos + edge.Vertices[1].Pos);
        }

        #endregion

        #region Opposites

        foreach (var edge in Edges)
        {
            edge.Opposite = FromVertices(edge.Vertices[0].Opposite, edge.Vertices[1].Opposite)!;
        }

        #endregion
    }

    private CubeEdge(byte ordinal, params CubeVertex[] vertices)
    {
        Ordinal = ordinal;
        Vertices = vertices;
    }

    public static CubeEdge? FromVertices(CubeVertex a, CubeVertex b)
    {
        return FromVerticesArr[a.Ordinal, b.Ordinal];
    }
}
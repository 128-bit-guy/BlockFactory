using OpenTK.Mathematics;

namespace BlockFactory.CubeMath;

public class CubeElement
{
    public static readonly CubeElement[] Elements;

    private static readonly CubeElement[] FromVertexArr;
    private static readonly CubeElement[] FromEdgeArr;
    private static readonly CubeElement[] FromFaceArr;

    public static readonly CubeElement FullCube = null!;
    public readonly byte Ordinal;
    public readonly Vector3i Pos;

    static CubeElement()
    {
        #region Elements

        Elements = new CubeElement[27];
        for (byte i = 0; i < 27; ++i) Elements[i] = new CubeElement(i);

        #endregion

        #region Typed elements

        foreach (var element in Elements)
        {
            element.Type = (CubeElementType)(3 - (element.Pos - Vector3i.One).ManhattanLength);
            switch (element.Type)
            {
                case CubeElementType.Vertex:
                    element.Vertex = CubeVertex.FromVector(element.Pos / 2);
                    break;
                case CubeElementType.Edge:
                    element.Edge = CubeEdge.Edges.OrderBy(e => (e.Center - (Vector3)element.Pos / 2).Length).First();
                    break;
                case CubeElementType.Face:
                    element.Face = DirectionUtils.FromVector(element.GetOffset());
                    break;
            }
        }

        #endregion

        #region From typed elements

        FromVertexArr = new CubeElement[CubeVertex.Vertices.Length];
        FromEdgeArr = new CubeElement[CubeEdge.Edges.Length];
        FromFaceArr = new CubeElement[DirectionUtils.GetValues().Length];

        foreach (var element in Elements)
            switch (element.Type)
            {
                case CubeElementType.Vertex:
                    FromVertexArr[element.Vertex!.Ordinal] = element;
                    break;
                case CubeElementType.Edge:
                    FromEdgeArr[element.Edge!.Ordinal] = element;
                    break;
                case CubeElementType.Face:
                    FromFaceArr[(int)element.Face!.Value] = element;
                    break;
                case CubeElementType.FullCube:
                    FullCube = element;
                    break;
            }

        #endregion
    }

    private CubeElement(byte ordinal)
    {
        Ordinal = ordinal;
        Pos = new Vector3i(ordinal % 3, ordinal / 3 % 3, ordinal / 9 % 3);
    }

    public CubeVertex? Vertex { get; private set; }
    public CubeEdge? Edge { get; private set; }
    public Direction? Face { get; private set; }
    public CubeElementType Type { get; private set; }

    public static CubeElement? FromVector(Vector3i vec)
    {
        if (vec.X is >= 0 and <= 2 && vec.Y is >= 0 and <= 2 && vec.Z is >= 0 and <= 2)
            return Elements[vec.X + vec.Y * 3 + vec.Z * 9];
        return null;
    }

    public Vector3i GetOffset()
    {
        return Pos - Vector3i.One;
    }

    public static CubeElement? FromOffset(Vector3i offset)
    {
        return FromVector(offset + Vector3i.One);
    }
}
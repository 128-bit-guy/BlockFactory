using OpenTK.Mathematics;

namespace BlockFactory.CubeMath;

public class CubeVertex
{
    public static readonly CubeVertex[] Vertices;
    public readonly byte Ordinal;
    public readonly Vector3i Pos;
    public CubeVertex Opposite { get; private set; }

    static CubeVertex()
    {
        Vertices = new CubeVertex[1 << 3];
        for (byte i = 0; i < (1 << 3); ++i)
        {
            Vertices[i] = new CubeVertex(i);
        }
        
        for (byte i = 0; i < (1 << 3); ++i)
        {
            Vertices[i].Opposite = FromVector(Vector3i.One - Vertices[i].Pos)!;
        }
    }

    private CubeVertex(byte ordinal)
    {
        Ordinal = ordinal;
        Pos = new Vector3i(ordinal & 1, (ordinal >> 1) & 1, (ordinal >> 2) & 1);
    }

    public static CubeVertex? FromVector(Vector3i pos)
    {
        if (pos.X is >= 0 and <= 1 && pos.Y is >= 0 and <= 1 && pos.Z is >= 0 and <= 1)
        {
            return Vertices[pos.X | (pos.Y << 1) | (pos.Z << 2)];
        }
        else
        {
            return null;
        }
    }
}
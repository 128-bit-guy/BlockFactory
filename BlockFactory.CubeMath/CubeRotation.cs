using OpenTK.Mathematics;

namespace BlockFactory.CubeMath;

public class CubeRotation
{
    public static readonly CubeRotation[] Rotations;
    private static readonly CubeRotation[,][] FromTo;
    private static readonly CubeRotation[,,] FromToKeeping;
    private readonly int[] _axisPermutation;
    private readonly int _signChangeMask;
    public readonly byte Ordinal;
    private CubeRotation[] _combinations = null!;
    private CubeFace[] _directionTransformations = null!;
    private CubeVertex[] _vertexTransformations = null!;
    private CubeEdge[] _edgeTransformations = null!;

    static CubeRotation()
    {
        var optionsToRotations = new Dictionary<(int, int, int, int), CubeRotation>();

        #region Rotations

        var rotationsList = new List<CubeRotation>();
        var axisPermutation = new[] { 0, 1, 2 };
        do
        {
            for (var signChangeMask = 0; signChangeMask < 1 << 3; ++signChangeMask)
            {
                var vecs = new Vector3[3];
                for (var i = 0; i < 3; ++i)
                {
                    var to = axisPermutation[i];
                    var signChange = (signChangeMask & (1 << to)) != 0;
                    vecs[i][to] = signChange ? -1 : 1;
                }

                var isRotation = Vector3.Dot(Vector3.Cross(vecs[0], vecs[1]), vecs[2]) > 0;
                if (isRotation)
                {
                    var rotation = new CubeRotation((byte)rotationsList.Count, (int[])axisPermutation.Clone(),
                        signChangeMask);
                    rotationsList.Add(rotation);
                    optionsToRotations[
                        (rotation._axisPermutation[0], rotation._axisPermutation[1], rotation._axisPermutation[2],
                            rotation._signChangeMask)] = rotation;
                }
            }
        } while (Algorithms.NextPermutation(axisPermutation));

        Rotations = rotationsList.ToArray();

        #endregion

        #region Combinations

        foreach (var rotation in Rotations)
        {
            rotation._combinations = new CubeRotation[Rotations.Length];
            foreach (var rotation1 in Rotations)
            {
                var newOptions = Combine(rotation, rotation1);
                var result = optionsToRotations[newOptions];
                rotation._combinations[rotation1.Ordinal] = result;
            }
        }

        #endregion

        #region Direction transformations

        foreach (var rotation in Rotations)
        {
            rotation._directionTransformations = new CubeFace[CubeFaceUtils.GetValues().Length];
            foreach (var direction in CubeFaceUtils.GetValues())
                rotation._directionTransformations[(int)direction] = rotation.TransformDirection(direction);
        }

        #endregion

        #region Inverses

        foreach (var a in Rotations)
        foreach (var b in Rotations)
            if (a * b == Rotations[0])
                a.Inverse = b;

        #endregion

        #region FromTo

        FromTo = new CubeRotation[CubeFaceUtils.GetValues().Length, CubeFaceUtils.GetValues().Length][];
        foreach (var a in CubeFaceUtils.GetValues())
        foreach (var b in CubeFaceUtils.GetValues())
            FromTo[(int)a, (int)b] = Rotations.Where(rotation => rotation * a == b).ToArray();

        #endregion

        #region FromToKeeping

        FromToKeeping = new CubeRotation[CubeFaceUtils.GetValues().Length, CubeFaceUtils.GetValues().Length,
            CubeFaceUtils.GetValues().Length];
        foreach (var a in CubeFaceUtils.GetValues())
        foreach (var b in CubeFaceUtils.GetValues())
        foreach (var c in CubeFaceUtils.GetValues())
        foreach (var rotation in GetFromTo(a, b))
            if (rotation * c == c)
            {
                FromToKeeping[(int)a, (int)b, (int)c] = rotation;
                break;
            }

        #endregion

        #region Matrices

        foreach (var rotation in Rotations)
        {
            rotation.Matrix = new Matrix3(rotation * Vector3.UnitX, rotation * Vector3.UnitY, rotation * Vector3.UnitZ);
            var mat4 = new Matrix4(rotation.Matrix)
            {
                [3, 3] = 1f
            };
            rotation.Matrix4 = mat4;
            rotation.AroundCenterRotation = Matrix4.CreateTranslation(-0.5f, -0.5f, -0.5f) * rotation.Matrix4 *
                                            Matrix4.CreateTranslation(0.5f, 0.5f, 0.5f);
        }

        #endregion

        #region Vertices

        foreach (var rotation in Rotations)
        {
            rotation._vertexTransformations = new CubeVertex[1 << 3];
            foreach (var vertex in CubeVertex.Vertices)
            {
                rotation._vertexTransformations[vertex.Ordinal] =
                    CubeVertex.FromVector(rotation.RotateAroundCenter(vertex.Pos))!;
            }
        }

        #endregion

        #region Edges

        foreach (var rotation in Rotations)
        {
            rotation._edgeTransformations = new CubeEdge[CubeEdge.Edges.Length];
            foreach (var edge in CubeEdge.Edges)
            {
                rotation._edgeTransformations[edge.Ordinal] =
                    CubeEdge.FromVertices(rotation * edge.Vertices[0], rotation * edge.Vertices[1])!;
            }
        }

        #endregion
    }

    private CubeRotation(byte ordinal, int[] axisPermutation, int signChangeMask)
    {
        Ordinal = ordinal;
        _axisPermutation = axisPermutation;
        _signChangeMask = signChangeMask;
    }

    public CubeRotation Inverse { get; private set; } = null!;
    public Matrix3 Matrix { get; private set; }
    public Matrix4 Matrix4 { get; private set; }
    public Matrix4 AroundCenterRotation { get; private set; }

    public static CubeRotation[] GetFromTo(CubeFace a, CubeFace b)
    {
        return FromTo[(int)a, (int)b];
    }

    public static CubeRotation GetFromToKeeping(CubeFace a, CubeFace b, CubeFace keeping)
    {
        return FromToKeeping[(int)a, (int)b, (int)keeping];
    }

    public static CubeRotation operator *(CubeRotation a, CubeRotation b)
    {
        return a._combinations[b.Ordinal];
    }

    public static Vector3i operator *(CubeRotation a, Vector3i b)
    {
        Vector3i result = default;
        for (var i = 0; i < 3; ++i) result[a._axisPermutation[i]] = b[i];
        for (var i = 0; i < 3; ++i)
            if ((a._signChangeMask & (1 << i)) != 0)
                result[i] = -result[i];
        return result;
    }

    public static Vector3 operator *(CubeRotation a, Vector3 b)
    {
        Vector3 result = default;
        for (var i = 0; i < 3; ++i) result[a._axisPermutation[i]] = b[i];
        for (var i = 0; i < 3; ++i)
            if ((a._signChangeMask & (1 << i)) != 0)
                result[i] = -result[i];
        return result;
    }

    public Vector3i RotateAroundCenter(Vector3i vec)
    {
        var abs = vec.BitShiftLeft(1) - new Vector3i(1, 1, 1);
        var absRotated = this * abs;
        return (absRotated + new Vector3i(1, 1, 1)).BitShiftRight(1);
    }

    public Vector3 RotateAroundCenter(Vector3 vec)
    {
        var abs = vec - new Vector3(0.5f, 0.5f, 0.5f);
        var absRotated = this * abs;
        return absRotated + new Vector3(0.5f, 0.5f, 0.5f);
    }

    private CubeFace TransformDirection(CubeFace face)
    {
        var axis = face.GetAxis();
        var sign = face.GetSign();
        var newAxis = _axisPermutation[axis];
        var changeSign = (_signChangeMask & (1 << newAxis)) != 0;
        var newSign = changeSign ? -sign : sign;
        return CubeFaceUtils.FromAxisAndSign(newAxis, newSign);
    }

    public static CubeFace operator *(CubeRotation a, CubeFace b)
    {
        return a._directionTransformations[(int)b];
    }

    private bool ChangesSign(int i)
    {
        return (_signChangeMask & (1 << _axisPermutation[i])) != 0;
    }

    private static (int, int, int, int) Combine(CubeRotation a, CubeRotation b)
    {
        var newAxisPermutation = new int[3];
        var newSignChangeMask = 0;
        for (var i = 0; i < 3; ++i)
        {
            newAxisPermutation[i] = a._axisPermutation[b._axisPermutation[i]];
            var signChange = b.ChangesSign(i) != a.ChangesSign(b._axisPermutation[i]);
            if (signChange) newSignChangeMask |= 1 << newAxisPermutation[i];
        }

        return (newAxisPermutation[0], newAxisPermutation[1], newAxisPermutation[2], newSignChangeMask);
    }

    public static CubeVertex operator *(CubeRotation rotation, CubeVertex vertex)
    {
        return rotation._vertexTransformations[vertex.Ordinal];
    }

    public static CubeEdge operator *(CubeRotation rotation, CubeEdge edge)
    {
        return rotation._edgeTransformations[edge.Ordinal];
    }
}
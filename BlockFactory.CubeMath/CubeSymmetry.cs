using Silk.NET.Maths;

namespace BlockFactory.CubeMath;

public class CubeSymmetry
{
    public static readonly CubeSymmetry[] Values;

    private static readonly CubeSymmetry[][] FromTo;

    private static readonly CubeSymmetry?[] FromToKeepingRotations;

    public static readonly CubeSymmetry Identity;

    private static readonly CubeSymmetry[] Inverses;

    private readonly CubeFace[] _faceTransformations;

    public readonly Matrix4X4<float> AroundCenterMatrix4;

    public readonly bool IsRotation;

    public readonly bool IsSymmetry;

    public readonly Matrix3X3<int> Matrix;

    public readonly Matrix4X4<float> Matrix4;

    public readonly int Ordinal;

    private CubeSymmetry[] _combinations = null!;

    static CubeSymmetry()
    {
        #region BasicGeneration

        var values = new List<CubeSymmetry>();
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
        {
            if (i == j) continue;
            for (var k = 0; k < 3; ++k)
            {
                if (i == k || j == k) continue;

                for (var s = 0; s < 8; ++s)
                {
                    var mat = new Matrix3X3<int>();
                    var si = ((s & 1) << 1) - 1;
                    var sj = (((s >> 1) & 1) << 1) - 1;
                    var sk = (((s >> 2) & 1) << 1) - 1;
                    mat.Row1.SetValue(i, si);
                    mat.Row2.SetValue(j, sj);
                    mat.Row3.SetValue(k, sk);
                    values.Add(new CubeSymmetry(values.Count, mat));
                }
            }
        }

        Values = values.ToArray();

        #endregion

        #region Combinations

        foreach (var symmetry in Values) symmetry._combinations = new CubeSymmetry[Values.Length];

        var symmetriesByMatrices = new Dictionary<Matrix3X3<int>, CubeSymmetry>();
        foreach (var symmetry in Values!) symmetriesByMatrices[symmetry.Matrix] = symmetry;

        foreach (var symmetry in Values)
        foreach (var symmetry2 in Values)
        {
            var combinationMatrix = symmetry.Matrix * symmetry2.Matrix;
            var combination = symmetriesByMatrices[combinationMatrix];
            symmetry._combinations[symmetry2.Ordinal] = combination;
        }

        #endregion

        #region FromTo

        FromTo = new CubeSymmetry[6 * 6 * 2][];
        foreach (var a in CubeFaceUtils.Values())
        foreach (var b in CubeFaceUtils.Values())
            for (var onlyRotationsInt = 0; onlyRotationsInt < 2; ++onlyRotationsInt)
            {
                var onlyRotations = onlyRotationsInt == 1;
                FromTo[(int)a + (int)b * 6 + onlyRotationsInt * 36] =
                    (from r in Values
                        where !onlyRotations || r.IsRotation
                        where a * r == b
                        select r).ToArray();
            }

        #endregion

        #region FromToKeepingRotations

        FromToKeepingRotations = new CubeSymmetry?[6 * 6 * 6];

        foreach (var a in CubeFaceUtils.Values())
        foreach (var b in CubeFaceUtils.Values())
        foreach (var keep in CubeFaceUtils.Values())
            FromToKeepingRotations[(int)a + (int)b * 6 + (int)keep * 36] =
                GetFromTo(a, b, true).FirstOrDefault(s => keep * s == keep);

        #endregion

        #region Identity

        foreach (var rotation in Values)
        {
            var ok = CubeFaceUtils.Values().All(face => face * rotation == face);
            if (!ok) continue;
            Identity = rotation;
            break;
        }

        #endregion

        #region Inverses

        Inverses = new CubeSymmetry[Values.Length];
        foreach (var symmetry in Values)
        {
            foreach(var oSymmetry in Values)
            {
                if (symmetry * oSymmetry != Identity) continue;
                Inverses[symmetry.Ordinal] = oSymmetry;
                break;
            }
        }

        #endregion
    }

    private CubeSymmetry(int ordinal, Matrix3X3<int> matrix)
    {
        Ordinal = ordinal;
        Matrix = matrix;
        IsRotation = matrix.GetDeterminant() == 1;
        IsSymmetry = !IsRotation;
        var facesFromVectors = new Dictionary<Vector3D<int>, CubeFace>();
        foreach (var face in CubeFaceUtils.Values()) facesFromVectors[face.GetDelta()] = face;

        _faceTransformations = new CubeFace[CubeFaceUtils.Values().Length];
        foreach (var face in CubeFaceUtils.Values())
            _faceTransformations[(int)face] = facesFromVectors[face.GetDelta() * matrix];

        Matrix4 = Matrix4X4<float>.Identity;
        for (var i = 0; i < 3; ++i)
        for (var j = 0; j < 3; ++j)
            Matrix4.SetValue(i, j, Matrix[i, j]);

        AroundCenterMatrix4 = Matrix4X4.CreateTranslation(-0.5f, -0.5f, -0.5f) * Matrix4 *
                              Matrix4X4.CreateTranslation(0.5f, 0.5f, 0.5f);
    }

    public CubeSymmetry Inverse => Inverses[Ordinal];

    #region Application

    public static CubeFace operator *(CubeFace face, CubeSymmetry symmetry)
    {
        return symmetry._faceTransformations[(int)face];
    }

    public static Vector3D<int> operator *(Vector3D<int> vector, CubeSymmetry symmetry)
    {
        return vector * symmetry.Matrix;
    }

    public static CubeSymmetry operator *(CubeSymmetry a, CubeSymmetry b)
    {
        return a._combinations[b.Ordinal];
    }

    public Vector3D<float> TransformAroundCenter(Vector3D<float> v)
    {
        return Vector3D.Transform(v, AroundCenterMatrix4);
    }

    public Box3D<float> TransformAroundCenter(Box3D<float> b)
    {
        var rotMin = TransformAroundCenter(b.Min);
        var rotMax = TransformAroundCenter(b.Max);
        return new Box3D<float>(Vector3D.Min(rotMin, rotMax), Vector3D.Max(rotMin, rotMax));
    }

    #endregion

    #region Casts

    public static explicit operator int(CubeSymmetry symmetry)
    {
        return symmetry.Ordinal;
    }

    public static explicit operator CubeSymmetry(int x)
    {
        return Values[x];
    }

    #endregion

    #region Finders

    public static CubeSymmetry[] GetFromTo(CubeFace a, CubeFace b, bool onlyRotations = false)
    {
        var onlyRotationsInt = onlyRotations ? 1 : 0;
        return FromTo[(int)a + (int)b * 6 + onlyRotationsInt * 36];
    }

    public static CubeSymmetry? GetFromToKeepingRotation(CubeFace a, CubeFace b, CubeFace keep)
    {
        return FromToKeepingRotations[(int)a + (int)b * 6 + (int)keep * 36];
    }

    #endregion
}
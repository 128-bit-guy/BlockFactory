using BlockFactory.Side_;
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public readonly struct FrustumIntersectionHelper
{
    private readonly float nxX, nxY, nxZ, nxW;
    private readonly float pxX, pxY, pxZ, pxW;
    private readonly float nyX, nyY, nyZ, nyW;
    private readonly float pyX, pyY, pyZ, pyW;
    private readonly float nzX, nzY, nzZ, nzW;
    private readonly float pzX, pzY, pzZ, pzW;
    public FrustumIntersectionHelper(Matrix4 m)
    {
        nxX = m[0, 3] + m[0, 0]; nxY = m[1, 3] + m[1, 0]; nxZ = m[2, 3] + m[2, 0]; nxW = m[3, 3] + m[3, 0];
        pxX = m[0, 3] - m[0, 0]; pxY = m[1, 3] - m[1, 0]; pxZ = m[2, 3] - m[2, 0]; pxW = m[3, 3] - m[3, 0];
        nyX = m[0, 3] + m[0, 1]; nyY = m[1, 3] + m[1, 1]; nyZ = m[2, 3] + m[2, 1]; nyW = m[3, 3] + m[3, 1];
        pyX = m[0, 3] - m[0, 1]; pyY = m[1, 3] - m[1, 1]; pyZ = m[2, 3] - m[2, 1]; pyW = m[3, 3] - m[3, 1];
        nzX = m[0, 3] + m[0, 2]; nzY = m[1, 3] + m[1, 2]; nzZ = m[2, 3] + m[2, 2]; nzW = m[3, 3] + m[3, 2];
        pzX = m[0, 3] - m[0, 2]; pzY = m[1, 3] - m[1, 2]; pzZ = m[2, 3] - m[2, 2]; pzW = m[3, 3] - m[3, 2];
    }

    public FrustumIntersectionHelper(VPMatrices matrices) : this(matrices.View * matrices.Projection)
    {

    }

    public bool TestAab(float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
    {
        return nxX * (nxX < 0 ? minX : maxX) + nxY * (nxY < 0 ? minY : maxY) + nxZ * (nxZ < 0 ? minZ : maxZ) >= -nxW &&
               pxX * (pxX < 0 ? minX : maxX) + pxY * (pxY < 0 ? minY : maxY) + pxZ * (pxZ < 0 ? minZ : maxZ) >= -pxW &&
               nyX * (nyX < 0 ? minX : maxX) + nyY * (nyY < 0 ? minY : maxY) + nyZ * (nyZ < 0 ? minZ : maxZ) >= -nyW &&
               pyX * (pyX < 0 ? minX : maxX) + pyY * (pyY < 0 ? minY : maxY) + pyZ * (pyZ < 0 ? minZ : maxZ) >= -pyW &&
               nzX * (nzX < 0 ? minX : maxX) + nzY * (nzY < 0 ? minY : maxY) + nzZ * (nzZ < 0 ? minZ : maxZ) >= -nzW &&
               pzX * (pzX < 0 ? minX : maxX) + pzY * (pzY < 0 ? minY : maxY) + pzZ * (pzZ < 0 ? minZ : maxZ) >= -pzW;
    }

    public bool TestAab(Vector3 min, Vector3 max)
    {
        return TestAab(min.X, min.Y, min.Z, max.X, max.Y, max.Z);
    }

    public bool TestAab(Box3 box)
    {
        return TestAab(box.Min, box.Max);
    }
}
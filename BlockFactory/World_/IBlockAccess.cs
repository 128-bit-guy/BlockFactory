using Silk.NET.Maths;

namespace BlockFactory.World_;

public interface IBlockAccess
{
    public short GetBlock(Vector3D<int> pos);
}
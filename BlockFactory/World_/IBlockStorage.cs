using Silk.NET.Maths;

namespace BlockFactory.World_;

public interface IBlockStorage : IBlockAccess
{
    public void SetBlock(Vector3D<int> pos, short block, bool update = true);
}
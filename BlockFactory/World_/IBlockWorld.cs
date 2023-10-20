using Silk.NET.Maths;

namespace BlockFactory.World_;

public interface IBlockWorld : IBlockStorage
{
    public void UpdateBlock(Vector3D<int> pos);
}
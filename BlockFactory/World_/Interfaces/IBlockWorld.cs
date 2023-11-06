using Silk.NET.Maths;

namespace BlockFactory.World_.Interfaces;

public interface IBlockWorld : IBlockStorage
{
    public void UpdateBlock(Vector3D<int> pos);
    public void ScheduleLightUpdate(Vector3D<int> pos);
}
using BlockFactory.Content.BlockInstance_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_.Interfaces;

public interface IBlockAccess
{
    public short GetBlock(Vector3D<int> pos);
    public byte GetBiome(Vector3D<int> pos);
    public byte GetLight(Vector3D<int> pos, LightChannel channel);
    public bool IsBlockLoaded(Vector3D<int> pos);
    public float GetDayCoefficient();
    public BlockInstance? GetBlockInstance(Vector3D<int> pos);
}
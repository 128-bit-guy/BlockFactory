using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class EmptyWorld : IBlockWorld
{
    public static readonly EmptyWorld Instance = new();
    public short GetBlock(Vector3D<int> pos)
    {
        return 0;
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        return 0;
    }

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return 15;
    }

    public bool IsBlockLoaded(Vector3D<int> pos)
    {
        return true;
    }

    public float GetDayCoefficient()
    {
        return 1;
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = true)
    {
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
    }

    public void UpdateBlock(Vector3D<int> pos)
    {
    }

    public void ScheduleLightUpdate(Vector3D<int> pos)
    {
    }
}
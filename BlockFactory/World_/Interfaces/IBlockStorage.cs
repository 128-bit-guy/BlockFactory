﻿using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_.Interfaces;

public interface IBlockStorage : IBlockAccess
{
    public void SetBlock(Vector3D<int> pos, short block, bool update = true);
    public void SetBiome(Vector3D<int> pos, byte biome);
    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light);
}
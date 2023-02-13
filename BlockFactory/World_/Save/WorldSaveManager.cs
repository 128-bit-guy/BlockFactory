﻿using OpenTK.Mathematics;

namespace BlockFactory.World_.Save;

public class WorldSaveManager : IDisposable
{
    public readonly string RegionDir;
    public readonly string SaveDir;
    public readonly World World;
    private readonly Dictionary<Vector3i, ChunkRegion> _regions;

    public WorldSaveManager(World world, string saveName)
    {
        World = world;
        SaveDir = World.GameInstance.SaveLocation + Path.DirectorySeparatorChar + saveName;
        RegionDir = SaveDir + Path.DirectorySeparatorChar + "regions";
        _regions = new Dictionary<Vector3i, ChunkRegion>();
    }

    public void Dispose()
    {
        
    }

    public string GetRegionSaveLocation(Vector3i pos)
    {
        return RegionDir + Path.DirectorySeparatorChar + $"region_{pos.X}_{pos.Y}_{pos.Z}.dat";
    }
}
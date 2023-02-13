using OpenTK.Mathematics;

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
        SaveDir = saveName;
        RegionDir = SaveDir + Path.DirectorySeparatorChar + "regions";
        Directory.CreateDirectory(RegionDir);
        _regions = new Dictionary<Vector3i, ChunkRegion>();
    }

    private void UnloadRegions(bool all)
    {
        var posesToRemove = new List<Vector3i>();
        foreach (var (pos, region) in _regions)
        {
            if (region.DependencyCount != 0 && !all) continue;
            region.EnsureUnloading();

            if (all)
            {
                region.UnloadTask!.Wait();
            }

            if (region.UnloadTask!.IsCompleted)
            {
                posesToRemove.Add(pos);
            }
        }

        foreach (var pos in posesToRemove)
        {
            _regions.Remove(pos);
        }
    }

    public void UnloadRegions()
    {
        UnloadRegions(false);
    }

    public void Dispose()
    {
        UnloadRegions(true);
    }

    public string GetRegionSaveLocation(Vector3i pos)
    {
        return RegionDir + Path.DirectorySeparatorChar + $"region_{pos.X}_{pos.Y}_{pos.Z}.dat";
    }

    public ChunkRegion GetRegion(Vector3i pos)
    {
        if (_regions.TryGetValue(pos, out var region))
        {
            return region;
        }

        region = new ChunkRegion(this, pos);
        region.RunLoadTask();
        _regions.Add(pos, region);
        return region;
    }
}
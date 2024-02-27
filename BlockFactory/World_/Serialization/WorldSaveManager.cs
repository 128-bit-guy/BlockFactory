using Silk.NET.Maths;

namespace BlockFactory.World_.Serialization;

public class WorldSaveManager : IDisposable
{
    private readonly Dictionary<Vector3D<int>, ChunkRegion> _regions = new();
    private readonly List<Vector3D<int>> _regionsToRemove = new();
    public readonly string SaveLocation;

    public WorldSaveManager(string saveLocation)
    {
        SaveLocation = saveLocation;
    }

    public void Dispose()
    {
        foreach (var region in _regions.Values)
        {
            region.LoadTask?.Wait();
            region.LoadTask = null;
            if (region.UnloadTask == null) region.StartUnloadTask();
        }

        foreach (var region in _regions.Values) region.UnloadTask!.Wait();

        _regions.Clear();
    }

    public ChunkRegion GetRegion(Vector3D<int> pos)
    {
        if (!_regions.TryGetValue(pos, out var region))
        {
            region = new ChunkRegion(pos, this);
            _regions[pos] = region;
            region.StartLoadTask();
        }

        region.UnloadTask?.Wait();
        region.UnloadTask = null;
        return region;
    }

    public void Update()
    {
        foreach (var region in _regions.Values)
        {
            if (region.LoadTask != null && region.LoadTask.IsCompleted) region.LoadTask = null;

            if (region.DependencyCount != 0) continue;
            if (region.LoadTask != null) continue;
            if (region.UnloadTask == null)
                region.StartUnloadTask();
            else if (region.UnloadTask.IsCompleted) _regionsToRemove.Add(region.Position);
        }

        foreach (var pos in _regionsToRemove) _regions.Remove(pos);

        _regionsToRemove.Clear();
    }

    public void CreateDirectory()
    {
        Directory.CreateDirectory(SaveLocation);
    }
}
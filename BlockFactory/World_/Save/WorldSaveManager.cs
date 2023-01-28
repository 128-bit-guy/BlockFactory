using OpenTK.Mathematics;
using BlockFactory.Game;

namespace BlockFactory.World_.Save;

public class WorldSaveManager : IDisposable
{
    public readonly World World;
    public readonly string SaveDir;
    public readonly string RegionDir;

    public WorldSaveManager(World world, string saveName)
    {
        World = world;
        SaveDir = World.GameInstance.SaveLocation + Path.DirectorySeparatorChar + saveName;
        RegionDir = SaveDir + Path.DirectorySeparatorChar + "regions";
    }

    public string GetRegionSaveLocation(Vector3i pos)
    {
        return RegionDir + Path.DirectorySeparatorChar + $"region_{pos.X}_{pos.Y}_{pos.Z}.dat";
    }

    public void Dispose()
    {
    }
}
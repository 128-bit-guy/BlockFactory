using System.Diagnostics;
using BlockFactory.Base;
using BlockFactory.Utils;
using BlockFactory.World_.ChunkLoading;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Search;

public abstract class WorldLocationSearcher : IDisposable
{
    private const int BoxRadius = PlayerChunkLoading.ExtensionChunkRadius;
    private const int BoxDiameter = 2 * BoxRadius + 1;

    private readonly Chunk?[,,] _chunkBox = new Chunk?[BoxDiameter, BoxDiameter, BoxDiameter];

    protected readonly IChunkStorage World;

    public Vector3D<int>? FoundPos { get; private set; }

    protected WorldLocationSearcher(IChunkStorage world)
    {
        World = world;
    }

    protected abstract Vector3D<int> GetPotentialChunkPos();

    protected abstract bool CheckPosition(BlockPointer pos);

    private void UnloadBoxIfNecessary()
    {
        if (_chunkBox[0, 0, 0] == null)
        {
            return;
        }

        for (var i = 0; i < BoxDiameter; ++i)
        for (var j = 0; j < BoxDiameter; ++j)
        for (var k = 0; k < BoxDiameter; ++k)
        {
            _chunkBox[i, j, k]!.RemoveTickingDependency();
            _chunkBox[i, j, k] = null;
        }
    }

    private void LoadBoxIfNecessary()
    {
        if (_chunkBox[0, 0, 0] != null)
        {
            return;
        }
        
        var center = GetPotentialChunkPos();

        for (var i = 0; i < BoxDiameter; ++i)
        for (var j = 0; j < BoxDiameter; ++j)
        for (var k = 0; k < BoxDiameter; ++k)
        {
            var absPos = new Vector3D<int>(i, j, k) - new Vector3D<int>(BoxRadius) + center;
            _chunkBox[i, j, k] = World.GetChunk(absPos);
            _chunkBox[i, j, k]!.AddTickingDependency();
        }
    }

    private bool IsLoadingComplete()
    {
        for (var i = BoxRadius - 1; i <= BoxRadius + 1; ++i)
        for (var j = BoxRadius - 1; j <= BoxRadius + 1; ++j)
        for (var k = BoxRadius - 1; k <= BoxRadius + 1; ++k)
        {
            if (!_chunkBox[i, j, k]!.IsLoaded) return false;
            if (!_chunkBox[i, j, k]!.Data!.FullyDecorated)
            {
                return false;
            }
        }

        return true;
    }

    private void CheckChunk()
    {
        var c = _chunkBox[BoxRadius, BoxRadius, BoxRadius]!;
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            var absPos = c.Position.ShiftLeft(Constants.ChunkSizeLog2) + new Vector3D<int>(i, j, k);
            if (CheckPosition(new BlockPointer(c.Neighbourhood, absPos)))
            {
                Console.WriteLine($"Found suitable pos: {absPos}");
                FoundPos = absPos;
                return;
            }
        }
    }

    public void Update()
    {
        if (FoundPos.HasValue)
        {
            UnloadBoxIfNecessary();
            return;
        }
        LoadBoxIfNecessary();
        if (IsLoadingComplete())
        {
            CheckChunk();
            if (!FoundPos.HasValue)
            {
                Console.WriteLine(
                    $"Failed to find suitable pos in chunk {_chunkBox[BoxRadius, BoxRadius, BoxRadius]!.Position}"
                    );
            }
            UnloadBoxIfNecessary();
        }

        if (!FoundPos.HasValue)
        {
            LoadBoxIfNecessary();
        }
    }

    public void Dispose()
    {
        UnloadBoxIfNecessary();
    }
}
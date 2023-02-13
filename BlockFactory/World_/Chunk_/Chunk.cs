using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Util.Dependency;
using BlockFactory.World_.Api;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Chunk_;

public class Chunk : IBlockStorage, IDependable
{
    public readonly Vector3i Pos;
    public readonly World World;
    private ChunkData? _data;
    private int _dependencyCount;
    public ChunkNeighbourhood Neighbourhood;
    public bool ReadyForTick;
    public int ReadyForUseNeighbours;
    public Dictionary<long, PlayerEntity> ViewingPlayers = new();
    public ChunkGenerationLevel VisitedGenerationLevel;

    public Chunk(ChunkData data, Vector3i pos, World world) : this(pos, world)
    {
        Data = data;
    }

    public Chunk(Vector3i pos, World world)
    {
        Pos = pos;
        World = world;
        _dependencyCount = 0;
        ReadyForUseNeighbours = 0;
        ReadyForTick = false;
        Neighbourhood = new ChunkNeighbourhood(this);
    }

    public ChunkData? Data
    {
        get => _data;
        set
        {
            _data = value;
            if (_data == null)
                VisitedGenerationLevel = ChunkGenerationLevel.Exists;
            else
                VisitedGenerationLevel = _data._generationLevel;
        }
    }

    public BlockState GetBlockState(Vector3i pos)
    {
        if (IsInside(pos))
        {
            EnsureMinLevel(World.GetGenerationLevel());
            var beg = GetBegin();
            var cur = pos - beg;
            return Data!.GetBlockState(cur);
        }

        return World.GetBlockState(pos);
    }

    public void SetBlockState(Vector3i pos, BlockState state)
    {
        if (IsInside(pos))
        {
            EnsureMinLevel(World.GetGenerationLevel());
            var beg = GetBegin();
            var cur = pos - beg;
            var prevState = Data!.GetBlockState(cur);
            if (prevState != state)
            {
                Data!.SetBlockState(cur, state);
                foreach (var (_, player) in ViewingPlayers) player.VisibleBlockChanged(this, pos, prevState, state);
            }
        }
        else
        {
            World.SetBlockState(pos, state);
        }
    }

    public ref int DependencyCount => ref _dependencyCount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3i GetBegin()
    {
        return Pos.BitShiftLeft(Constants.ChunkSizeLog2);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInside(Vector3i pos)
    {
        var beg = GetBegin();
        var cur = pos - beg;
        for (var i = 0; i < 3; ++i)
            if (cur[i] < 0 || cur[i] >= Constants.ChunkSize)
                return false;
        return true;
    }

    public void EnsureMinLevel(ChunkGenerationLevel level, bool process = true)
    {
        if (Data!._generationLevel < level)
        {
            EnsureMinLevel(level - 1, false);
            World.Generator.UpgradeChunkToLevel(level, this);
            Data!._generationLevel = level;
            if (process) World.Generator.ProcessScheduled();
        }
    }

    public Box3i GetInclusiveBox()
    {
        var begin = GetBegin();
        return new Box3i(begin, begin + new Vector3i(Constants.ChunkSize - 1));
    }
}
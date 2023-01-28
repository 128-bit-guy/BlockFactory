using OpenTK.Mathematics;
using System.Runtime.CompilerServices;
using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Util.Dependency;
using BlockFactory.World_.Api;
using BlockFactory.Entity_;
using BlockFactory.Util.Math_;

namespace BlockFactory.World_.Chunk_
{

    public class Chunk : IBlockStorage, IDependable
    {
        public const int SizeLog2 = 4;
        public const int Size = 1 << SizeLog2;
        public const int Mask = Size - 1;
        public ChunkData? Data;
        public readonly Vector3i Pos;
        public readonly World World;
        public int ReadyForUseNeighbours;
        public bool ReadyForTick;
        public ChunkNeighbourhood Neighbourhood;
        public Dictionary<long, PlayerEntity> ViewingPlayers = new();
        private int _dependencyCount;

        public ref int DependencyCount => ref _dependencyCount;

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

        public BlockState GetBlockState(Vector3i pos)
        {
            if (IsInside(pos))
            {
                EnsureMinLevel(World.GetGenerationLevel());
                Vector3i beg = GetBegin();
                Vector3i cur = pos - beg;
                return Data!.GetBlockState(cur);
            }
            else
            {
                return World.GetBlockState(pos);
            }
        }

        public void SetBlockState(Vector3i pos, BlockState state)
        {
            if (IsInside(pos))
            {
                EnsureMinLevel(World.GetGenerationLevel());
                Vector3i beg = GetBegin();
                Vector3i cur = pos - beg;
                BlockState prevState = Data!.GetBlockState(cur);
                if (prevState != state)
                {
                    Data!.SetBlockState(cur, state);
                    foreach (var (_, player) in ViewingPlayers)
                    {
                        player.VisibleBlockChanged(this, pos, prevState, state);
                    }
                }
            }
            else
            {
                World.SetBlockState(pos, state);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3i GetBegin()
        {
            return Pos.BitShiftLeft(SizeLog2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInside(Vector3i pos)
        {
            Vector3i beg = GetBegin();
            Vector3i cur = pos - beg;
            for (int i = 0; i < 3; ++i)
            {
                if (cur[i] < 0 || cur[i] >= Size)
                {
                    return false;
                }
            }
            return true;
        }

        public void EnsureMinLevel(ChunkGenerationLevel level, bool process = true)
        {
            if (Data!._generationLevel < level)
            {
                EnsureMinLevel(level - 1, false);
                World.Generator.UpgradeChunkToLevel(level, this);
                Data!._generationLevel = level;
                if (process)
                {
                    World.Generator.ProcessScheduled();
                }
            }
        }

        public Box3i GetInclusiveBox()
        {
            Vector3i begin = GetBegin();
            return new Box3i(begin, begin + new Vector3i(Size - 1));
        }
    }
}
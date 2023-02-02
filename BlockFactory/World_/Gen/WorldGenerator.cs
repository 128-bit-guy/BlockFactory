using BlockFactory.Block_;
using BlockFactory.CubeMath;
using BlockFactory.Game;
using BlockFactory.Init;
using BlockFactory.Side_;
using BlockFactory.Util;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;
using SharpNoise.Modules;

namespace BlockFactory.World_.Gen;

public class WorldGenerator
{
    private readonly List<Chunk>[] _chunkDistribution = new List<Chunk>[3 * 3 * 3];
    private readonly List<Chunk>[,,] _componentChunks = new List<Chunk>[3, 3, 3];

    private readonly ChunkGenerationLevel[] _generationLevels;
    private readonly List<Chunk>[] _scheduledUpgrades;
    public readonly int Seed;

    [ExclusiveTo(Side.Server)] private long _chunksUpgradedOnOtherThread;

    [ExclusiveTo(Side.Server)] private long _totalChunksUpgraded;

    public int ChunksUpgraded;
    public Perlin Perlin;

    public WorldGenerator(int seed)
    {
        Seed = seed;
        Perlin = new Perlin
        {
            Seed = unchecked(1798783699 * seed + 1675735027),
            Frequency = 2f,
            Lacunarity = 2f,
            Persistence = 2f,
            OctaveCount = 3
        };
        ChunksUpgraded = 0;
        _generationLevels = Enum.GetValues<ChunkGenerationLevel>();
        _scheduledUpgrades = new List<Chunk>[_generationLevels.Length];
        for (var i = 0; i < _generationLevels.Length; ++i)
            foreach (var rem in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
                _scheduledUpgrades[i] = new List<Chunk>();

        for (var i = 0; i < _chunkDistribution.Length; ++i) _chunkDistribution[i] = new List<Chunk>();

        foreach (var pos in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            _componentChunks[pos.X, pos.Y, pos.Z] = new List<Chunk>();
    }

    [ExclusiveTo(Side.Server)]
    public double OtherThreadRatio => (double)_chunksUpgradedOnOtherThread / _totalChunksUpgraded;

    public void UpgradeChunkToLevel(ChunkGenerationLevel level, Chunk chunk)
    {
        if (Thread.CurrentThread != chunk.World.GameInstance.MainThread)
            throw new InvalidOperationException("Can not upgrade chunk from not main thread!");
        ++ChunksUpgraded;
        chunk.World.PushGenerationLevel(level - 1);
        //chunk.EnsureMinLevel(level - 1);
        foreach (var oPos in new Box3i(chunk.Pos - new Vector3i(1), chunk.Pos + new Vector3i(1)).InclusiveEnumerable())
            chunk.World.GetOrLoadChunk(oPos, false);
        //chunk.SetBlockState(chunk.Pos.BitShiftLeft(Chunk.SizeLog2), new BlockState(Blocks.Stone, CubeRotation.Rotations[2]));
        chunk.World.PopGenerationLevel();
        _scheduledUpgrades[(int)level].Add(chunk);
    }

    private void AddComponentChunks(Chunk chunk, ChunkGenerationLevel level, List<Chunk>[,,] componentChunks)
    {
        if (chunk.VisitedGenerationLevel >= level || chunk.Data!._generationLevel < level) return;
        chunk.VisitedGenerationLevel = level;
        var rem = chunk.Pos;
        for (var i = 0; i < 3; ++i)
        {
            rem[i] %= 3;
            if (rem[i] < 0) rem[i] += 3;
        }

        componentChunks[rem.X, rem.Y, rem.Z].Add(chunk);
        foreach (var offset in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
        {
            if (offset == new Vector3i(1)) continue;
            var neighbour = chunk.Neighbourhood.Chunks[offset.X, offset.Y, offset.Z];
            if (neighbour == null) continue;
            // AddComponentChunks(neighbour, level, componentChunks);
            foreach (var offset2 in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
            {
                var neighbour2 = neighbour.Neighbourhood.Chunks[offset2.X, offset2.Y, offset2.Z];
                if (neighbour2 != null) AddComponentChunks(neighbour2, level, componentChunks);
            }
        }
    }

    public void ProcessScheduled()
    {
        if (_scheduledUpgrades[(int)ChunkGenerationLevel.SurfaceGenerated].Count != 0)
        {
            var ch = _scheduledUpgrades[(int)ChunkGenerationLevel.SurfaceGenerated][0];
            ch.World.PushGenerationLevel(ChunkGenerationLevel.Exists);
            Parallel.ForEach(_scheduledUpgrades[(int)ChunkGenerationLevel.SurfaceGenerated], GenerateBaseSurface);
            ch.World.PopGenerationLevel();
        }

        foreach (var generationLevel in _generationLevels)
        {
            var mapos = 0;
            foreach (var chunk in _scheduledUpgrades[(int)generationLevel])
            {
                AddComponentChunks(chunk, generationLevel, _componentChunks);
                var cpos = 0;
                foreach (var rem in new Box3i(new Vector3i(0), new Vector3i(2)).InclusiveEnumerable())
                {
                    var remChunks = _componentChunks[rem.X, rem.Y, rem.Z];
                    if (remChunks.Count != 0) _chunkDistribution[cpos++].AddRange(remChunks);
                    remChunks.Clear();
                }

                mapos = Math.Max(mapos, cpos);
            }

            if (mapos != 0) _chunkDistribution[0][0].World.PushGenerationLevel(generationLevel - 1);
            for (var i = 0; i < mapos; ++i)
                Parallel.ForEach(_chunkDistribution[i], c => ActuallyUpgradeChunkToLevel(generationLevel, c));
            if (mapos != 0) _chunkDistribution[0][0].World.PopGenerationLevel();

            for (var i = 0; i < mapos; ++i) _chunkDistribution[i].Clear();
            _scheduledUpgrades[(int)generationLevel].Clear();
        }
    }

    [ExclusiveTo(Side.Server)]
    private void IncrementStats(Chunk chunk)
    {
        Interlocked.Increment(ref _totalChunksUpgraded);
        if (Thread.CurrentThread != chunk.World.GameInstance.MainThread)
            Interlocked.Increment(ref _chunksUpgradedOnOtherThread);
    }

    private void ActuallyUpgradeChunkToLevel(ChunkGenerationLevel level, Chunk chunk)
    {
        if (chunk.World.GameInstance.Kind == GameKind.MultiplayerBackend) IncrementStats(chunk);

        switch (level)
        {
            case ChunkGenerationLevel.SurfaceGenerated:
                GenerateSurface(chunk);
                break;
            case ChunkGenerationLevel.TopSoilPlaced:
                PlaceTopSoil(chunk);
                break;
            case ChunkGenerationLevel.Decorated:
                Decorate(chunk);
                break;
        }
    }

    private Random GetChunkRandom(Vector3i pos, int a, int b, int c, int d)
    {
        return new Random(unchecked(pos.X * a + pos.Y * b + pos.Z * c + d * Seed));
    }

    private void GenerateBaseSurface(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1401634909, 1527589979, 1057394087, 1642081541);
        for (var x = 0; x < Chunk.Size; ++x)
        {
            var absX = chunk.GetBegin().X + x;
            for (var z = 0; z < Chunk.Size; ++z)
            {
                var absZ = chunk.GetBegin().Z + z;
                var noise = Perlin.GetValue(absX / 100f, 0, absZ / 100f);
                for (var y = 0; y < Chunk.Size; ++y)
                {
                    var absY = chunk.GetBegin().Y + y;
                    if (noise >= absY)
                        chunk.Neighbourhood.SetBlockState((absX, absY, absZ), new BlockState(Blocks.Stone,
                            RandomRotations.Any(random)));
                }
            }
        }
    }

    private void GenerateSurface(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1718678431, 1225666907, 1640233921, 1794161587);
        foreach (var a in chunk.GetInclusiveBox().InclusiveEnumerable())
            if (a.Y >= 50 && a.Y <= 100 && random.Next(30000) == 0)
                foreach (var b in WorldEnumerators.GetSphereEnumerator(a, random.Next(5, 8)))
                    chunk.Neighbourhood.SetBlockState(b, new BlockState(Blocks.Stone,
                        RandomRotations.Any(random)));
    }

    private void PlaceTopSoil(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1454274037, 1016482381, 1497360727, 1925636137);
        foreach (var a in chunk.GetInclusiveBox().InclusiveEnumerable())
            if ((chunk.Neighbourhood.GetBlockState(a).Block == Blocks.Stone ||
                 chunk.Neighbourhood.GetBlockState(a).Block == Blocks.Dirt) &&
                chunk.Neighbourhood.GetBlockState(a + new Vector3i(0, 1, 0)).Block == Blocks.Air)
            {
                var oPos = a;
                while (true)
                {
                    if (a.Y - oPos.Y >= 15) break;
                    if (chunk.Neighbourhood.GetBlockState(oPos).Block == Blocks.Stone ||
                        chunk.Neighbourhood.GetBlockState(oPos).Block == Blocks.Dirt)
                    {
                        if (oPos == a)
                            chunk.Neighbourhood.SetBlockState(oPos, new BlockState(Blocks.Grass,
                                RandomRotations.KeepingUp(random)));
                        else
                            chunk.Neighbourhood.SetBlockState(oPos, new BlockState(Blocks.Dirt,
                                RandomRotations.Any(random)));
                        if (random.Next(3) <= 1)
                            oPos -= new Vector3i(0, 1, 0);
                        else
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
    }

    private void Decorate(Chunk chunk)
    {
        var random = GetChunkRandom(chunk.Pos, 1514407261, 1177000723, 1707795989, 1133321689);
        foreach (var a in chunk.GetInclusiveBox().InclusiveEnumerable())
        {
            var downState = chunk.Neighbourhood.GetBlockState(a - Vector3i.UnitY);
            if (downState.Block == Blocks.Grass && downState.Rotation * Direction.Up == Direction.Up)
                if (random.Next(100) == 0)
                {
                    var height = random.Next(5, 8);
                    for (var i = 0; i < height; ++i)
                    {
                        var state = new BlockState(Blocks.Log, RandomRotations.KeepingY(random));
                        chunk.Neighbourhood.SetBlockState(a + Vector3i.UnitY * i, state);
                    }

                    var logTop = a + Vector3i.UnitY * (height - 1);
                    foreach (var pos in WorldEnumerators.GetSphereEnumerator(logTop,
                                 random.Next(3, Math.Min(height - 1, 7))))
                        if (chunk.Neighbourhood.GetBlockState(pos).Block == Blocks.Air)
                            chunk.Neighbourhood.SetBlockState(pos, new BlockState(Blocks.Leaves,
                                RandomRotations.Any(random)));
                }
        }
    }
}
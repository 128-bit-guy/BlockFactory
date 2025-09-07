using BlockFactory.Base;
using BlockFactory.Content.Biome_;
using BlockFactory.Content.Block_;
using BlockFactory.Utils;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class MazeWorldGenerator : IWorldGenerator
{
    private WorldGenElement _walls;
    public MazeWorldGenerator(long seed)
    {
        Seed = seed;
        _walls = new WorldGenElement(this, 1577629357);
    }

    private bool isBlockWall(Vector2D<int> pos)
    {
        int cx = pos.X >> Constants.ChunkSizeLog2;
        int cz = pos.Y >> Constants.ChunkSizeLog2;
        int cntEven = (cx & 1) + (cz & 1);
        if (cntEven == 0) return false;
        if (cntEven == 2) return true;
        return _walls.GetPosRandom(new Vector3D<int>(cx, 0, cz)).Next(2) == 0;
    }

    public long Seed { get; }
    public void GenerateChunk(Chunk c)
    {
        c.Data = new ChunkData();
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            int absX = (c.Position.X << Constants.ChunkSizeLog2) + i;
            int absZ = (c.Position.Z << Constants.ChunkSizeLog2) + k;
            bool wall = isBlockWall(new Vector2D<int>(absX, absZ));
            int maxY = wall ? 20 : 0;
            for (var j = 0; j < Constants.ChunkSize; ++j)
            {
                var absPos = c.Position.ShiftLeft(Constants.ChunkSizeLog2) + new Vector3D<int>(i, j, k);
                if (absPos.Y == maxY)
                {
                    c.Data!.SetBlock(absPos, Blocks.Grass);
                }
                else if (absPos.Y == maxY - 1)
                {
                    c.Data!.SetBlock(absPos, Blocks.Dirt);
                }
                else if (absPos.Y < maxY - 1)
                {
                    c.Data!.SetBlock(absPos, 1);
                    c.Data!.SetBiome(absPos, Biomes.Underground);
                }
                else
                {
                    c.Data!.SetBiome(absPos, Biomes.Surface);
                }
            }
        }

        var sky = c.Position.Y > 0;

        if (sky)
        {
            var cnt = 0;
            for (var i = 0; i < Constants.ChunkSize; ++i)
            for (var j = 0; j < Constants.ChunkSize; ++j)
            for (var k = 0; k < Constants.ChunkSize; ++k)
            {
                var mi = Math.Min(Math.Min(i, j), k);
                var ma = Math.Max(i, k);
                var border = mi == 0 || ma == Constants.ChunkSize - 1;
                if (border)
                {
                    ++cnt;
                    c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, j, k), true);
                }
                else
                {
                    c.Data.SetLight(new Vector3D<int>(i, j, k), LightChannel.DirectSky, 15);
                    c.Data.SetLight(new Vector3D<int>(i, j, k), LightChannel.Sky, 15);
                }
            }
        }
        else
        {
            for (var i = 0; i < Constants.ChunkSize; ++i)
            for (var j = 0; j < Constants.ChunkSize; ++j)
                c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, Constants.ChunkSize - 1, j), true);
        }
    }

    public void DecorateChunk(Chunk c)
    {
        
    }

    public string GetName()
    {
        return "Maze";
    }
}
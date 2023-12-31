﻿using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Math_;
using BlockFactory.Random_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class WorldGenerator
{
    private readonly BaseTerrainGenerator _baseTerrainGenerator = new();
    private readonly CaveGenerator _caveGenerator;
    private readonly WorldDecorator _decorator;

    public WorldGenerator()
    {
        _caveGenerator = new CaveGenerator(this);
        _decorator = new WorldDecorator(this);
    }

    public long Seed => 0;

    public void GenerateChunk(Chunk c)
    {
        c.Data = new ChunkData();
        _baseTerrainGenerator.GenerateBaseTerrain(c);
        _caveGenerator.GenerateCaves(c);
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        {
            c.Data.SetLightUpdateScheduled(new Vector3D<int>(i, Constants.ChunkSize - 1, j), true);
        }
    }

    public void DecorateChunk(Chunk c)
    {
        _decorator.DecorateChunk(c);
    }
}
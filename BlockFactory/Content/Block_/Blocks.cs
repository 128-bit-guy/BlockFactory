﻿using System.Diagnostics.CodeAnalysis;
using BlockFactory.Registry_;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace BlockFactory.Content.Block_;

[SuppressMessage("Usage", "CA2211")]
public static class Blocks
{
    public static Registry<Block> Registry;
    public static AirBlock Air;
    public static SimpleBlock Stone;
    public static BricksBlock Bricks;
    public static DirtBlock Dirt;
    public static GrassBlock Grass;
    public static ColumnBlock Log;
    public static SimpleBlock IronOre;
    public static SimpleBlock CopperOre;
    public static SimpleBlock TinOre;
    public static SimpleBlock DiamondOre;
    public static SimpleBlock CoalOre;
    public static LeavesBlock Leaves;
    public static WaterBlock Water;
    public static SandBlock Sand;
    public static Block Planks;
    public static FenceBlock Fence;
    public static TorchBlock Torch;
    public static TallGrassBlock TallGrass;

    public static void Init()
    {
        Registry = SynchronizedRegistries.NewSynchronizedRegistry<Block>("Block");
        Air = Registry.RegisterForced("Air", 0, new AirBlock());
        Stone = Registry.RegisterForced("Stone", 1, new SimpleBlock(0));
        Bricks = Registry.Register("Bricks", new BricksBlock());
        Dirt = Registry.Register("Dirt", new DirtBlock());
        Grass = Registry.Register("Grass", new GrassBlock());
        Log = Registry.Register("Log", new ColumnBlock(7, 6, 5));
        IronOre = Registry.Register("IronOre", new SimpleBlock(18, Stone));
        CopperOre = Registry.Register("CopperOre", new SimpleBlock(19, Stone));
        TinOre = Registry.Register("TinOre", new SimpleBlock(20, Stone));
        DiamondOre = Registry.Register("DiamondOre", new SimpleBlock(21, Stone));
        CoalOre = Registry.Register("CoalOre", new SimpleBlock(22, Stone));
        Leaves = Registry.Register("Leaves", new LeavesBlock());
        Water = Registry.Register("Water", new WaterBlock());
        Sand = Registry.Register("Sand", new SandBlock());
        Planks = Registry.Register("Planks", new SimpleBlock(11));
        Fence = Registry.Register("Fence", new FenceBlock());
        Torch = Registry.Register("Torch", new TorchBlock());
        TallGrass = Registry.Register("TallGrass", new TallGrassBlock());
    }

    public static void Lock()
    {
        Registry.Lock();
    }
}
using BlockFactory.Content.Biome_;
using BlockFactory.Content.Block_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public struct BlockPointer
{
    public readonly IBlockWorld World;
    public readonly Vector3D<int> Pos;

    public BlockPointer(IBlockWorld world, Vector3D<int> pos)
    {
        World = world;
        Pos = pos;
    }

    public static BlockPointer operator +(BlockPointer pointer, Vector3D<int> pos)
    {
        return new BlockPointer(pointer.World, pointer.Pos + pos);
    }

    public static BlockPointer operator -(BlockPointer pointer, Vector3D<int> pos)
    {
        return new BlockPointer(pointer.World, pointer.Pos - pos);
    }

    public short GetBlock()
    {
        return World.GetBlock(Pos);
    }

    public Block GetBlockObj()
    {
        return World.GetBlockObj(Pos);
    }

    public byte GetBiome()
    {
        return World.GetBiome(Pos);
    }

    public Biome GetBiomeObj()
    {
        return World.GetBiomeObj(Pos);
    }

    public byte GetLight(LightChannel channel)
    {
        return World.GetLight(Pos, channel);
    }

    public void SetBlock(short block, bool update = true)
    {
        World.SetBlock(Pos, block, update);
    }

    public void SetBlock(Block block, bool update = true)
    {
        World.SetBlock(Pos, block, update);
    }

    public void SetBiome(byte biome)
    {
        World.SetBiome(Pos, biome);
    }

    public void SetBiome(Biome biome)
    {
        World.SetBiome(Pos, biome);
    }

    public void SetLight(LightChannel channel, byte light)
    {
        World.SetLight(Pos, channel, light);
    }

    public static implicit operator ConstBlockPointer(BlockPointer pointer)
    {
        return new ConstBlockPointer(pointer.World, pointer.Pos);
    }
}
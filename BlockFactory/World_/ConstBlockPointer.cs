using BlockFactory.Content.Biome_;
using BlockFactory.Content.Block_;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public struct ConstBlockPointer
{
    public readonly IBlockAccess World;
    public readonly Vector3D<int> Pos;

    public ConstBlockPointer(IBlockAccess world, Vector3D<int> pos)
    {
        World = world;
        Pos = pos;
    }

    public static ConstBlockPointer operator +(ConstBlockPointer pointer, Vector3D<int> pos)
    {
        return new ConstBlockPointer(pointer.World, pointer.Pos + pos);
    }

    public static ConstBlockPointer operator -(ConstBlockPointer pointer, Vector3D<int> pos)
    {
        return new ConstBlockPointer(pointer.World, pointer.Pos - pos);
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

    public static explicit operator BlockPointer(ConstBlockPointer pointer)
    {
        return new BlockPointer((IBlockWorld)pointer.World, pointer.Pos);
    }
}
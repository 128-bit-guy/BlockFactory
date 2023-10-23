using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Serialization;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Serialization;

public class ChunkData : IBlockStorage, ITagSerializable
{
    private short[] _blocks = new short[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];
    private byte[] _biomes = new byte[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];
    public bool Decorated;

    public short GetBlock(Vector3D<int> pos)
    {
        return _blocks[GetArrIndex(pos)];
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        return _biomes[GetArrIndex(pos)];
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = false)
    {
        _blocks[GetArrIndex(pos)] = block;
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        _biomes[GetArrIndex(pos)] = biome;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & Constants.ChunkMask) | (((pos.Y & Constants.ChunkMask) | ((pos.Z & Constants.ChunkMask)
            << Constants.ChunkSizeLog2)) << Constants.ChunkSizeLog2);
    }

    public DictionaryTag SerializeToTag()
    {
        var res = new DictionaryTag();
        res.SetValue("blocks", _blocks);
        res.SetValue("biomes", _biomes);
        res.SetValue("decorated", Decorated);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        _blocks = tag.GetValue<short[]>("blocks");
        _biomes = tag.GetValue<byte[]>("biomes");
        Decorated = tag.GetValue<bool>("decorated");
    }
}
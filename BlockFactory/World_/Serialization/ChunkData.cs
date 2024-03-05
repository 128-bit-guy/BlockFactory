using System.Collections;
using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Serialization;
using BlockFactory.World_.Interfaces;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.World_.Serialization;

public class ChunkData : IBlockStorage, IBinarySerializable
{
    private static readonly int LightChannelCnt = Enum.GetValues<LightChannel>().Length;
    private readonly byte[] _biomes = new byte[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];
    private readonly short[] _blocks = new short[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];

    private readonly byte[] _light =
        new byte[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize * LightChannelCnt];

    private BitArray _blockUpdateScheduled = new(Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize);

    private BitArray _lightUpdateScheduled = new(Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize);

    public bool Decorated;
    public int DecoratedNeighbours;
    public bool HasSkyLight;
    public bool TopSoilPlaced;
    public bool FullyDecorated => DecoratedNeighbours == 27;

    public void SerializeBinary(BinaryWriter writer, SerializationReason reason)
    {
        foreach (var block in _blocks) writer.Write(block);

        writer.Write(_biomes);

        writer.Write(_light);

        if (reason != SerializationReason.Save) return;

        var lightUpdateScheduled = new byte[_lightUpdateScheduled.Length >> 3];
        _lightUpdateScheduled.CopyTo(lightUpdateScheduled, 0);

        writer.Write(lightUpdateScheduled);

        var blockUpdateScheduled = new byte[_blockUpdateScheduled.Length >> 3];
        _blockUpdateScheduled.CopyTo(blockUpdateScheduled, 0);

        writer.Write(blockUpdateScheduled);

        writer.Write(Decorated);

        writer.Write(HasSkyLight);

        writer.Write7BitEncodedInt(DecoratedNeighbours);

        writer.Write(TopSoilPlaced);
    }

    public void DeserializeBinary(BinaryReader reader, SerializationReason reason)
    {
        for (var i = 0; i < _blocks.Length; ++i) _blocks[i] = reader.ReadInt16();

        for (var i = 0; i < _biomes.Length; ++i) _biomes[i] = reader.ReadByte();

        for (var i = 0; i < _light.Length; ++i) _light[i] = reader.ReadByte();

        if (reason != SerializationReason.Save)
        {
            Decorated = HasSkyLight = true;
            return;
        }

        _lightUpdateScheduled = new BitArray(reader.ReadBytes(_lightUpdateScheduled.Length >> 3));

        _blockUpdateScheduled = new BitArray(reader.ReadBytes(_blockUpdateScheduled.Length >> 3));

        Decorated = reader.ReadBoolean();

        HasSkyLight = reader.ReadBoolean();

        DecoratedNeighbours = reader.Read7BitEncodedInt();

        TopSoilPlaced = reader.ReadBoolean();
    }

    public short GetBlock(Vector3D<int> pos)
    {
        return _blocks[GetArrIndex(pos)];
    }

    public byte GetBiome(Vector3D<int> pos)
    {
        return _biomes[GetArrIndex(pos)];
    }

    public byte GetLight(Vector3D<int> pos, LightChannel channel)
    {
        return _light[GetArrIndex(pos) | ((int)channel << (3 * Constants.ChunkSizeLog2))];
    }

    public bool IsBlockLoaded(Vector3D<int> pos)
    {
        return true;
    }

    public void SetBlock(Vector3D<int> pos, short block, bool update = false)
    {
        _blocks[GetArrIndex(pos)] = block;
    }

    public void SetBiome(Vector3D<int> pos, byte biome)
    {
        _biomes[GetArrIndex(pos)] = biome;
    }

    public void SetLight(Vector3D<int> pos, LightChannel channel, byte light)
    {
        _light[GetArrIndex(pos) | ((int)channel << (3 * Constants.ChunkSizeLog2))] = light;
    }

    public bool IsLightUpdateScheduled(Vector3D<int> pos)
    {
        return _lightUpdateScheduled[GetArrIndex(pos)];
    }

    public bool IsBlockUpdateScheduled(Vector3D<int> pos)
    {
        return _blockUpdateScheduled[GetArrIndex(pos)];
    }

    public void SetLightUpdateScheduled(Vector3D<int> pos, bool update)
    {
        _lightUpdateScheduled[GetArrIndex(pos)] = update;
    }

    public void SetBlockUpdateScheduled(Vector3D<int> pos, bool update)
    {
        _blockUpdateScheduled[GetArrIndex(pos)] = update;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & Constants.ChunkMask) | (((pos.Y & Constants.ChunkMask) | ((pos.Z & Constants.ChunkMask)
            << Constants.ChunkSizeLog2)) << Constants.ChunkSizeLog2);
    }
}
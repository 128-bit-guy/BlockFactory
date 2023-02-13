using BlockFactory.Base;

namespace BlockFactory.Serialization.Tag;

public class ChunkBlockDataTag : ITag
{
    public TagType Type => TagType.ChunkBlockData;
    public ushort[,,] Data;

    public ChunkBlockDataTag() : this(new ushort[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize])
    {
        
    }

    public ChunkBlockDataTag(ushort[,,] data)
    {
        Data = data;
    }
    public void Write(BinaryWriter writer)
    {
        
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            writer.Write(Data[i, j, k]);
        }

    }

    public void Read(BinaryReader reader)
    {
        for (var i = 0; i < Constants.ChunkSize; ++i)
        for (var j = 0; j < Constants.ChunkSize; ++j)
        for (var k = 0; k < Constants.ChunkSize; ++k)
        {
            Data[i, j, k] = reader.ReadUInt16();
        }

    }
}
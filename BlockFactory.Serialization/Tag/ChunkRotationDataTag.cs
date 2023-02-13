using BlockFactory.Base;

namespace BlockFactory.Serialization.Tag;

public class ChunkRotationDataTag : ITag
{
    public TagType Type => TagType.ChunkBlockData;
    public byte[,,] Data;

    public ChunkRotationDataTag() : this(new byte[Constants.ChunkSize, Constants.ChunkSize, Constants.ChunkSize])
    {
        
    }

    public ChunkRotationDataTag(byte[,,] data)
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
            Data[i, j, k] = reader.ReadByte();
        }

    }
}
using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Serialization;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;
using OpenTK.Mathematics;

namespace BlockFactory.Util.Math_;

public struct EntityPos : ITagSerializable
{
    public Vector3 PosInChunk;
    public Vector3i ChunkPos;

    public EntityPos(Vector3 posInChunk, Vector3i chunkPos)
    {
        PosInChunk = posInChunk;
        ChunkPos = chunkPos;
    }


    public EntityPos(Vector3 absolutePos) : this(absolutePos, (0, 0, 0))
    {
        Fix();
    }

    public EntityPos(Vector3i blockPos) : this(blockPos.BitAnd(Constants.ChunkMask).ToVector3(),
        blockPos.BitShiftRight(Constants.ChunkSizeLog2))
    {
    }

    public EntityPos(BinaryReader reader)
    {
        PosInChunk = NetworkUtils.ReadVector3(reader);
        ChunkPos = NetworkUtils.ReadVector3i(reader);
    }

    public void Write(BinaryWriter writer)
    {
        PosInChunk.Write(writer);
        ChunkPos.Write(writer);
    }

    public void Fix()
    {
        for (var i = 0; i < 3; ++i)
        {
            var posInChunk = PosInChunk[i];
            var deltaChunkPos = (int)MathF.Floor(posInChunk / Constants.ChunkSize);
            posInChunk %= Constants.ChunkSize;
            if (posInChunk < 0) posInChunk += Constants.ChunkSize;
            PosInChunk[i] = posInChunk;
            ChunkPos[i] += deltaChunkPos;
        }
    }

    public static EntityPos operator +(EntityPos a, Vector3 b)
    {
        return new EntityPos(a.PosInChunk + b, a.ChunkPos);
    }

    public static EntityPos operator -(EntityPos a, Vector3 b)
    {
        return a + -b;
    }

    public Vector3i GetBlockPos()
    {
        return PosInChunk.Floor() + ChunkPos.BitShiftLeft(Constants.ChunkSizeLog2);
    }

    public Vector3 GetAbsolutePos()
    {
        return PosInChunk + ChunkPos.BitShiftLeft(Constants.ChunkSizeLog2).ToVector3();
    }

    public static EntityPos operator -(EntityPos a, EntityPos b)
    {
        EntityPos ep = new(a.PosInChunk - b.PosInChunk, a.ChunkPos - b.ChunkPos);
        ep.Fix();
        return ep;
    }
    // public void FromTag(CompoundTag tag)
    // {
    //     PosInChunk.X = tag.GetSingle("pos_in_chunk_x");
    //     PosInChunk.Y = tag.GetSingle("pos_in_chunk_y");
    //     PosInChunk.Z = tag.GetSingle("pos_in_chunk_z");
    //     ChunkPos.X = tag.GetInt32("chunk_pos_x");
    //     ChunkPos.Y = tag.GetInt32("chunk_pos_y");
    //     ChunkPos.Z = tag.GetInt32("chunk_pos_z");
    // }
    //
    // public CompoundTag ToTag()
    // {
    //     CompoundTag tag = new CompoundTag();
    //     tag.SetSingle("pos_in_chunk_x", PosInChunk.X);
    //     tag.SetSingle("pos_in_chunk_y", PosInChunk.Y);
    //     tag.SetSingle("pos_in_chunk_z", PosInChunk.Z);
    //     tag.SetInt32("chunk_pos_x", ChunkPos.X);
    //     tag.SetInt32("chunk_pos_y", ChunkPos.Y);
    //     tag.SetInt32("chunk_pos_z", ChunkPos.Z);
    //
    //     return tag;
    // }
    public DictionaryTag SerializeToTag()
    {
        var tag = new DictionaryTag();
        tag.Set("PosInChunk", PosInChunk.SerializeToTag());
        tag.Set("ChunkPos", ChunkPos.SerializeToTag());
        return tag;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        PosInChunk = VectorSerialization.DeserializeV3(tag.Get<ListTag>("PosInChunk"));
        ChunkPos = VectorSerialization.DeserializeV3I(tag.Get<ListTag>("ChunkPos"));
    }
}
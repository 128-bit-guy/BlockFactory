using BlockFactory.Util;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Network
{
    public class ChunkDataPacket : IPacket
    {
        public readonly Vector3i Pos;
        public readonly ChunkData Data;
        public ChunkDataPacket(Vector3i pos, ChunkData data)
        {
            Pos = pos;
            Data = data;
        }
        public ChunkDataPacket(BinaryReader reader)
        {
            Pos = NetworkUtils.ReadVector3i(reader);
            Data = new ChunkData(reader);
        }
        public void Write(BinaryWriter writer)
        {
            Pos.Write(writer);
            Data.Write(writer);
        }
    }
}

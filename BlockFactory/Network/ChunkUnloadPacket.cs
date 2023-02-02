using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class ChunkUnloadPacket : IPacket
{
    public readonly Vector3i Pos;

    public ChunkUnloadPacket(Vector3i pos)
    {
        Pos = pos;
    }

    public ChunkUnloadPacket(BinaryReader reader)
    {
        Pos = NetworkUtils.ReadVector3i(reader);
    }

    public void Write(BinaryWriter writer)
    {
        Pos.Write(writer);
    }
}
using BlockFactory.Block_;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class BlockChangePacket : IPacket
{
    public readonly Vector3i Pos;
    public readonly BlockState State;

    public BlockChangePacket(BinaryReader reader)
    {
        Pos = NetworkUtils.ReadVector3i(reader);
        State = new BlockState(reader);
    }

    public BlockChangePacket(Vector3i pos, BlockState state)
    {
        Pos = pos;
        State = state;
    }

    public void Write(BinaryWriter writer)
    {
        Pos.Write(writer);
        State.Write(writer);
    }
}
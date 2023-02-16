using BlockFactory.Base;
using BlockFactory.Block_;
using BlockFactory.Client;
using BlockFactory.CubeMath;
using BlockFactory.Game;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class BlockChangePacket : IInGamePacket
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

    public void Process(NetworkConnection connection)
    {
        BlockFactoryClient.Instance.Player!.VisibleChunks[Pos.BitShiftRight(Constants.ChunkSizeLog2)]
            .SetBlockState(Pos, State);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
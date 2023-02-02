using BlockFactory.Client;
using BlockFactory.Game;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class ChunkUnloadPacket : IInGamePacket
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

    public void Process(NetworkConnection connection)
    {
        BlockFactoryClient.Instance.Player?.RemoveVisibleChunk(Pos);
        BlockFactoryClient.Instance.Player?.World!.RemoveChunk(Pos);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
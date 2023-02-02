using BlockFactory.Client;
using BlockFactory.Game;
using BlockFactory.Util;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class ChunkDataPacket : IPacket
{
    public readonly ChunkData Data;
    public readonly Vector3i Pos;

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

    public void Process(NetworkConnection connection)
    {
        var ch = new Chunk(Data, Pos, BlockFactoryClient.Instance.Player!.World!);
        BlockFactoryClient.Instance.Player!.World!.AddChunk(ch);
        BlockFactoryClient.Instance.Player!.AddVisibleChunk(ch);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
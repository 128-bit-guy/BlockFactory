using BlockFactory.Client;
using BlockFactory.Entity_;
using BlockFactory.Game;
using BlockFactory.Util;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class EntityChunkPosUpdatePacket : IInGamePacket
{
    private readonly long Id;
    private readonly Vector3i FirstPos;
    private readonly Vector3i SecondPos;
    public EntityChunkPosUpdatePacket(BinaryReader reader)
    {
        Id = reader.ReadInt64();
        FirstPos = NetworkUtils.ReadVector3i(reader);
        SecondPos = NetworkUtils.ReadVector3i(reader);
    }

    public EntityChunkPosUpdatePacket(long id, Vector3i firstPos, Vector3i secondPos)
    {
        Id = id;
        FirstPos = firstPos;
        SecondPos = secondPos;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);
        FirstPos.Write(writer);
        SecondPos.Write(writer);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }

    public void Process(NetworkConnection connection)
    {
        var w = BlockFactoryClient.Instance.GameInstance!.World;
        var a = w.GetOrLoadChunk(FirstPos);
        var b = w.GetOrLoadChunk(SecondPos);
        var e = a.Data.EntitiesInChunk[Id];
        a.Data.EntitiesInChunk.Remove(Id);
        e.Chunk = b;
        b.Data.EntitiesInChunk.Add(Id, e);
    }
}
using BlockFactory.Client;
using BlockFactory.Client.Entity_;
using BlockFactory.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Util;
using BlockFactory.Util.Math_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class EntityPosUpdatePacket : IInGamePacket
{
    public readonly long Id;
    public readonly EntityPos Pos;
    public readonly Vector3i ChunkPos;

    public EntityPosUpdatePacket(BinaryReader reader)
    {
        Pos = new EntityPos(reader);
        Id = reader.ReadInt64();
        ChunkPos = NetworkUtils.ReadVector3i(reader);
    }

    public EntityPosUpdatePacket(EntityPos pos, long id, Vector3i chunkPos)
    {
        Pos = pos;
        Id = id;
        ChunkPos = chunkPos;
    }

    public void Write(BinaryWriter writer)
    {
        Pos.Write(writer);
        writer.Write(Id);
        ChunkPos.Write(writer);
    }

    public void Process(NetworkConnection connection)
    {
        var c = BlockFactoryClient.Instance.GameInstance!.World.GetOrLoadChunk(ChunkPos);
        var e = c.GetEntity(Id);
        e.SetNewPos(Pos);
        // PlayerEntity? entity = BlockFactoryClient.Instance.Player;
        // if (entity != null)
        //     if (entity.Id == Id)
        //         entity.SetNewPos(Pos);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
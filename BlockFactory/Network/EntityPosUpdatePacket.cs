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
    public readonly List<(Vector3i, long, EntityPos)> PosUpdates;

    public EntityPosUpdatePacket(BinaryReader reader)
    {
        var cnt = reader.Read7BitEncodedInt();
        PosUpdates = new List<(Vector3i, long, EntityPos)>(new (Vector3i, long, EntityPos)[cnt]);
        for (var i = 0; i < cnt; ++i)
        {
            var chunkPos = NetworkUtils.ReadVector3i(reader);
            var id = reader.ReadInt64();
            var pos = new EntityPos(reader);
            PosUpdates[i] = (chunkPos, id, pos);
        }
    }

    public EntityPosUpdatePacket(List<(Vector3i, long, EntityPos)> posUpdates)
    {
        PosUpdates = posUpdates;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(PosUpdates.Count);
        foreach (var (chunkPos, id, pos) in PosUpdates)
        {
            chunkPos.Write(writer);
            writer.Write(id);
            pos.Write(writer);
        }
    }

    public void Process(NetworkConnection connection)
    {
        foreach (var (chunkPos, id, pos) in PosUpdates)
        {
            var c = BlockFactoryClient.Instance.GameInstance!.World.GetOrLoadChunk(chunkPos);
            var e = c.GetEntity(id);
            e.SetNewPos(pos);
        }
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
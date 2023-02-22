using BlockFactory.Game;
using BlockFactory.Util;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class EntityRemovedPacket : IInGamePacket
{
    private readonly Vector3i _chunkPos;
    private readonly long _id;
    public EntityRemovedPacket(BinaryReader reader)
    {
        _chunkPos = NetworkUtils.ReadVector3i(reader);
        _id = reader.ReadInt64();
    }

    public EntityRemovedPacket(Vector3i chunkPos, long id)
    {
        _chunkPos = chunkPos;
        _id = id;
    }
    public void Write(BinaryWriter writer)
    {
        _chunkPos.Write(writer);
        writer.Write(_id);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }

    public void Process(NetworkConnection connection)
    {
        var c = connection.GameInstance!.World.GetOrLoadGeneratedChunk(_chunkPos);
        c.RemoveEntity(c.GetEntity(_id));
    }
}
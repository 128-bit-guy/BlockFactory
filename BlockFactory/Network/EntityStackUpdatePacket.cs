using BlockFactory.Client;
using BlockFactory.Game;
using BlockFactory.Item_;
using BlockFactory.Util;
using OpenTK.Mathematics;

namespace BlockFactory.Network;

public class EntityStackUpdatePacket : IInGamePacket
{
    private readonly Vector3i _entityChunkPos;
    private readonly long _entityId;
    private int _type;
    private readonly ItemStack _stack;
    public EntityStackUpdatePacket(BinaryReader reader)
    {
        _entityChunkPos = NetworkUtils.ReadVector3i(reader);
        _entityId = reader.ReadInt64();
        _type = reader.Read7BitEncodedInt();
        _stack = new ItemStack(reader);
    }

    public EntityStackUpdatePacket(Vector3i pos, long entityId, int type, ItemStack stack)
    {
        _entityChunkPos = pos;
        _entityId = entityId;
        _type = type;
        _stack = stack;
    }
    public void Write(BinaryWriter writer)
    {
        _entityChunkPos.Write(writer);
        writer.Write(_entityId);
        writer.Write7BitEncodedInt(_type);
        _stack.Write(writer);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }

    public void Process(NetworkConnection connection)
    {
        var c = BlockFactoryClient.Instance.GameInstance!.World.GetOrLoadChunk(_entityChunkPos);
        var e = c.GetEntity(_entityId);
        e.OnStackUpdate(_type, _stack);
    }
}
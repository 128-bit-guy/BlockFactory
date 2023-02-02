using BlockFactory.Client;
using BlockFactory.Client.Entity_;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Util.Math_;

namespace BlockFactory.Network;

public class EntityPosUpdatePacket : IInGamePacket
{
    public readonly long Id;
    public readonly EntityPos Pos;

    public EntityPosUpdatePacket(BinaryReader reader)
    {
        Pos = new EntityPos(reader);
        Id = reader.ReadInt64();
    }

    public EntityPosUpdatePacket(EntityPos pos, long id)
    {
        Pos = pos;
        Id = id;
    }

    public void Write(BinaryWriter writer)
    {
        Pos.Write(writer);
        writer.Write(Id);
    }

    public void Process(NetworkConnection connection)
    {
        PlayerEntity? entity = BlockFactoryClient.Instance.Player;
        if (entity != null)
            if (entity.Id == Id)
                entity.SetNewPos(Pos);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
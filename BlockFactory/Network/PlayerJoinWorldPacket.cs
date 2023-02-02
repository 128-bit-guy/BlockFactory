using BlockFactory.Client;
using BlockFactory.Game;

namespace BlockFactory.Network;

public class PlayerJoinWorldPacket : IPacket
{
    public readonly long Id;

    public PlayerJoinWorldPacket(BinaryReader reader)
    {
        Id = reader.ReadInt64();
    }

    public PlayerJoinWorldPacket(long id)
    {
        Id = id;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Id);
    }

    public void Process(NetworkConnection connection)
    {
        if (BlockFactoryClient.Instance.Player != null) BlockFactoryClient.Instance.Player.Id = Id;
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
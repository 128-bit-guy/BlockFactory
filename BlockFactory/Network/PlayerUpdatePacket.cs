using BlockFactory.Client;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;

namespace BlockFactory.Network;

public class PlayerUpdatePacket : IInGamePacket
{
    public readonly int Number;
    public readonly PlayerUpdateType UpdateType;

    public PlayerUpdatePacket(BinaryReader reader)
    {
        UpdateType = (PlayerUpdateType)reader.ReadByte();
        Number = reader.Read7BitEncodedInt();
    }

    public PlayerUpdatePacket(PlayerUpdateType updateType, int number)
    {
        UpdateType = updateType;
        Number = number;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte)UpdateType);
        writer.Write7BitEncodedInt(Number);
    }

    public void Process(NetworkConnection connection)
    {
        BlockFactoryClient.Instance.Player!.HandlePlayerUpdate(UpdateType, Number);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
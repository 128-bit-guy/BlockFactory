using BlockFactory.Client;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;

namespace BlockFactory.Network;

public class PlayerUpdatePacket : IPacket
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
        connection.GameInstance!.EnqueueWork(() =>
        {
            BlockFactoryClient.Instance.Player!.HandlePlayerUpdate(this.UpdateType, this.Number);
        });
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
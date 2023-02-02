using BlockFactory.Entity_.Player;
using BlockFactory.Game;

namespace BlockFactory.Network;

public class PlayerActionPacket : IPacket
{
    public readonly PlayerActionType ActionType;
    public readonly int Number;

    public PlayerActionPacket(PlayerActionType actionType, int number)
    {
        ActionType = actionType;
        Number = number;
    }

    public PlayerActionPacket(BinaryReader reader)
    {
        ActionType = (PlayerActionType)reader.ReadByte();
        Number = reader.Read7BitEncodedInt();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte)ActionType);
        writer.Write7BitEncodedInt(Number);
    }

    public void Process(NetworkConnection connection)
    {
        connection.GameInstance!.EnqueueWork(() =>
            {
                ((PlayerEntity)connection.SideObject!).HandlePlayerAction(ActionType, Number);
            }
        );
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend;
    }
}
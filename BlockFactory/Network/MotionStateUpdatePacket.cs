using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Server.Entity_;

namespace BlockFactory.Network;

public class MotionStateUpdatePacket : IInGamePacket
{
    public readonly MotionState State;

    public MotionStateUpdatePacket(BinaryReader reader)
    {
        State = (MotionState)reader.Read7BitEncodedInt();
    }

    public MotionStateUpdatePacket(MotionState state)
    {
        State = state;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt((int)State);
    }

    public void Process(NetworkConnection connection)
    {
        ((ServerPlayerEntity)connection.SideObject!).MotionState = State;
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend;
    }
}
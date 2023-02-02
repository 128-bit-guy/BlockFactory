using BlockFactory.Game;
using BlockFactory.Server.Entity_;

namespace BlockFactory.Network;

public class MotionStateUpdatePacket : IPacket
{
    public readonly MotionState State;

    public MotionStateUpdatePacket(BinaryReader reader)
    {
        State = new MotionState(reader);
    }

    public MotionStateUpdatePacket(MotionState state)
    {
        State = state;
    }

    public void Write(BinaryWriter writer)
    {
        State.Write(writer);
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
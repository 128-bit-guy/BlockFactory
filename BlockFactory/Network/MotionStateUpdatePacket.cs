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
}
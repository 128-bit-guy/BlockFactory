namespace BlockFactory.Network;

public class SidedMemoryConnection : IMessageConnection
{
    public readonly MemoryConnection MemoryConnection;

    internal SidedMemoryConnection(Side side, MemoryConnection memoryConnection)
    {
        Side = side;
        MemoryConnection = memoryConnection;
    }

    public Side Side { get; }
}
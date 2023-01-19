namespace BlockFactory.Network;

public class SidedMemoryConnection : IMessageConnection
{
    public Side Side { get; }
    public readonly MemoryConnection MemoryConnection;

    internal SidedMemoryConnection(Side side, MemoryConnection memoryConnection)
    {
        Side = side;
        MemoryConnection = memoryConnection;
    }
}
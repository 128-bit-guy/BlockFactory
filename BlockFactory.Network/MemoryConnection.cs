using BlockFactory.Side_;

namespace BlockFactory.Network;

public class MemoryConnection
{
    private readonly SidedMemoryConnection[] _sidedConnections;

    public MemoryConnection()
    {
        _sidedConnections = new SidedMemoryConnection[2];
        for (var i = 0; i < 2; ++i) _sidedConnections[i] = new SidedMemoryConnection((Side)i, this);
    }

    public SidedMemoryConnection this[Side s] => _sidedConnections[(int)s];
}
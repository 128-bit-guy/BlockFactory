namespace BlockFactory.Network;

public class NetworkConnection : IMessageConnection
{
    public NetworkConnection(Side side)
    {
        Side = side;
    }

    public Side Side { get; }
}
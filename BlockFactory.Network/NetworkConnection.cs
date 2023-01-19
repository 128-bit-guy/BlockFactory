namespace BlockFactory.Network;

public class NetworkConnection : IMessageConnection
{
    public Side Side { get; }

    public NetworkConnection(Side side)
    {
        Side = side;
    }
}
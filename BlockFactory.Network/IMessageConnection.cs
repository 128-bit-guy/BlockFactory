using BlockFactory.Side_;

namespace BlockFactory.Network;

public interface IMessageConnection
{
    public Side Side { get; }
}
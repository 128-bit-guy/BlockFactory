namespace BlockFactory.Network;

/// <summary>
///     Interface, which should be implemented by all packets
///     Implementors should have constructor, which takes one argument of type BinaryReader
///     Constructor will be invoked on thread pool thread
/// </summary>
public interface IInGamePacket : IPacket
{
    public void Process(NetworkConnection connection);
}
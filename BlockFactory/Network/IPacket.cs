namespace BlockFactory.Network;

/// <summary>
///     Interface, which should be implemented by all packets
///     Implementors should have constructor, which takes one argument of type BinaryReader
///     Constructor will be invoked on thread pool thread
/// </summary>
public interface IPacket
{
    /// <summary>
    ///     Writes packet to BinaryWriter.
    ///     Will be invoked on thread pool thread
    /// </summary>
    /// <param name="writer">BinaryWriter where the packet will be written to</param>
    public void Write(BinaryWriter writer);
}
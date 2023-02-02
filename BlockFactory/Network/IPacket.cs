using BlockFactory.Game;

namespace BlockFactory.Network;

public interface IPacket
{
    /// <summary>
    ///     Writes packet to BinaryWriter.
    ///     Will be invoked on thread pool thread
    /// </summary>
    /// <param name="writer">BinaryWriter where the packet will be written to</param>
    void Write(BinaryWriter writer);

    bool SupportsGameKind(GameKind kind);
}
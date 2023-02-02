using BlockFactory.Util;

namespace BlockFactory.Network;

/// <summary>
///     Registry for all packets and packet handlers
/// </summary>
public class NetworkRegistry
{
    /// <summary>
    ///     Delegate, which will be used to handle packets.
    ///     Will be invoked on thread pool thread
    /// </summary>
    /// <typeparam name="T">Type of packet</typeparam>
    public delegate void PacketHandler<in T>(T packet, NetworkConnection connection) where T : IPacket;

    /// <summary>
    ///     Non generic version of PacketHandler
    /// </summary>
    public delegate void PacketHandlerNG(IPacket packet, NetworkConnection connection);

    private static readonly Dictionary<Type, int> PacketTypeToId = new();
    private static readonly List<Type> PacketTypes = new();
    private static readonly List<PacketReader> PacketReaders = new();
    private static readonly List<PacketHandlerNG?> PacketHandlers = new();
    private static readonly List<bool> IsPacketCompressed0 = new();

    /// <summary>
    ///     Registers packet type
    /// </summary>
    /// <typeparam name="T">Type of packet</typeparam>
    public static void Register<T>(bool compressed = false) where T : IPacket
    {
        var packetType = typeof(T);
        PacketTypeToId[packetType] = PacketTypes.Count;
        PacketTypes.Add(packetType);
        var constructor = packetType.GetConstructor(new[] { typeof(BinaryReader) })!;
        PacketReaders.Add(constructor.CreateDelegate<PacketReader>());
        IsPacketCompressed0.Add(compressed);
    }

    /// <summary>
    ///     Registers packet handler
    /// </summary>
    /// <param name="handler">Packet handler delegate</param>
    /// <typeparam name="T">Type of packet</typeparam>
    public static void RegisterHandler<T>(PacketHandler<T> handler) where T : IPacket
    {
        var id = GetPacketId<T>();
        while (PacketHandlers.Count <= id) PacketHandlers.Add(null);
        PacketHandlers[id] = (packet, connection) => handler((T)packet, connection);
    }

    /// <summary>
    ///     Gets handler for packet from id
    /// </summary>
    /// <param name="id">Id of packet</param>
    /// <returns>Handler of packet, or null, if it is not registered</returns>
    public static PacketHandlerNG? GetHandler(int id)
    {
        if (PacketHandlers.Count <= id)
            return null;
        return PacketHandlers[id];
    }

    /// <summary>
    ///     Gets packet type from id
    /// </summary>
    /// <param name="id">Id of packet</param>
    /// <returns>Type of packet</returns>
    public static Type GetPacketType(int id)
    {
        return PacketTypes[id];
    }

    /// <summary>
    ///     Gets packet id from type
    /// </summary>
    /// <param name="type">Type of packet</param>
    /// <returns>Id of packet</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static int GetPacketId(Type type)
    {
        return PacketTypeToId[type];
    }

    /// <summary>
    ///     Gets packet id from type
    /// </summary>
    /// <typeparam name="T">Type of packet</typeparam>
    /// <returns>Id of packet</returns>
    public static int GetPacketId<T>() where T : IPacket
    {
        return GetPacketId(typeof(T));
    }

    /// <summary>
    ///     Reads packet and id from BinaryReader
    /// </summary>
    /// <param name="reader">BinaryReader from which packet will be read</param>
    /// <param name="id">Id of packet</param>
    /// <returns>Packet and id of it</returns>
    public static IPacket ReadPacket(BinaryReader reader, int id)
    {
        return PacketReaders[id](reader);
    }

    /// <summary>
    ///     Writes packet to BinaryWriter
    /// </summary>
    /// <param name="packet">Packet which will be written</param>
    /// <param name="writer">BinaryWriter to which packet will be written</param>
    public static void WritePacket(IPacket packet, BinaryWriter writer)
    {
        packet.Write(writer);
    }

    /// <summary>
    ///     Checks if packet should be compressed
    /// </summary>
    /// <param name="id">Id of packet to check</param>
    /// <returns>True if packet should be compressed, false otherwise</returns>
    public static bool IsPacketCompressed(int id)
    {
        return IsPacketCompressed0[id];
    }

    private delegate IPacket PacketReader(BinaryReader reader);
}
using ENet.Managed;

namespace BlockFactory.Network;

public static class NetworkRegistry
{
    private static bool _locked;
    private static List<(string ns, Type t, Func<IPacket> creator)> _packetList = new();
    private static readonly List<bool> _isCompressed = new();

    public static void RegisterPacket<T>(string ns, Func<T> creator) where T : class, IPacket
    {
        if (_locked) throw new InvalidOperationException("Network registry is frozen");
        _packetList.Add((ns, typeof(T), creator));
        PacketDataHolder<T>.Flags = GetFlags(typeof(T));
        PacketDataHolder<T>.IsCompressed = typeof(T).GetCustomAttributes(true)
            .Any(a => a is CompressedPacketAttribute);
    }

    private static ENetPacketFlags GetFlags(Type t)
    {
        var attribute = (PacketFlagsAttribute)t.GetCustomAttributes(true)
            .FirstOrDefault(a => a is PacketFlagsAttribute)!;
        return attribute.Flags;
    }

    public static void RegisterPacket<T>(string ns) where T : class, IPacket, new()
    {
        RegisterPacket(ns, () => new T());
    }

    public static void Lock()
    {
        _locked = true;
        _packetList = _packetList.OrderBy(e => e.ns).ToList();
        for (var i = 0; i < _packetList.Count; ++i)
        {
            var t = _packetList[i].t;
            var holder = typeof(PacketDataHolder<>).MakeGenericType(t);
            var id = holder.GetField(nameof(PacketDataHolder<object>.Id))!;
            id.SetValue(null, i);
            _isCompressed.Add((bool)holder.GetField(nameof(PacketDataHolder<object>.IsCompressed))!
                .GetValue(null)!);
        }
    }

    public static int GetPacketTypeId<T>() where T : class, IPacket
    {
        return PacketDataHolder<T>.Id;
    }

    public static ENetPacketFlags GetPacketFlags<T>() where T : class, IPacket
    {
        return PacketDataHolder<T>.Flags;
    }

    public static IPacket CreatePacket(int id)
    {
        return _packetList[id].creator();
    }

    public static bool IsPacketCompressed<T>() where T : class, IPacket
    {
        return PacketDataHolder<T>.IsCompressed;
    }

    public static bool IsPacketCompressed(int id)
    {
        return _isCompressed[id];
    }

    private static class PacketDataHolder<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static int Id = -1;

        // ReSharper disable once StaticMemberInGenericType
        public static ENetPacketFlags Flags = 0;

        // ReSharper disable once StaticMemberInGenericType
        public static bool IsCompressed;
    }
}
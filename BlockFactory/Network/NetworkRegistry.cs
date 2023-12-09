using System.Runtime.CompilerServices;
using ENet.Managed;

namespace BlockFactory.Network;

public static class NetworkRegistry
{
    private static bool _locked = false;
    private static List<(string ns, Type t, Func<IPacket> creator)> _packetList = new();

    public static void RegisterPacket<T>(string ns, Func<T> creator) where T : class, IPacket
    {
        if (_locked) throw new InvalidOperationException("Network registry is frozen");
        _packetList.Add((ns, typeof(T), creator));
        PacketDataHolder<T>.Flags = GetFlags(typeof(T));
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
            var f = holder.GetField(nameof(PacketDataHolder<object>.Id))!;
            f.SetValue(null, i);
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

    private static class PacketDataHolder<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        public static int Id = -1;

        // ReSharper disable once StaticMemberInGenericType
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once ConvertToConstant.Local
        public static ENetPacketFlags Flags = 0;
    }
}
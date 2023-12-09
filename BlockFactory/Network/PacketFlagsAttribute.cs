using ENet.Managed;

namespace BlockFactory.Network;

[AttributeUsage(AttributeTargets.Class)]
public class PacketFlagsAttribute : Attribute
{
    public readonly ENetPacketFlags Flags;

    public PacketFlagsAttribute(ENetPacketFlags flags)
    {
        Flags = flags;
    }
}
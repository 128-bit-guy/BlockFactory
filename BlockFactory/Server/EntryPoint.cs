using BlockFactory.Server.Dedicated;
using BlockFactory.Side_;

namespace BlockFactory.Server;

[ExclusiveTo(Side.Server)]
public static class EntryPoint
{
    [SidedEntryPoint(Side.Server)]
    public static void ServerMain(string[] args)
    {
        new BlockFactoryDedicatedServer().Run();
    }
}
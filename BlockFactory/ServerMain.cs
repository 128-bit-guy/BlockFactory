using BlockFactory.Base;
using BlockFactory.Server;

namespace BlockFactory;

[ExclusiveTo(Side.Server)]
public static class ServerMain
{
    [EntryPoint(Side.Server)]
    public static void Main()
    {
        Console.WriteLine("Launching Server!");
        GameInfo.PhysicalSide = Side.Server;
        BlockFactoryServer.Run();
    }
}
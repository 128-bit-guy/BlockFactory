using BlockFactory.Base;
using BlockFactory.Client;

namespace BlockFactory;

[ExclusiveTo(Side.Client)]
public static class ClientMain
{
    [EntryPoint(Side.Client)]
    public static void Main()
    {
        Console.WriteLine("Launching Client!");
        BlockFactoryClient.Run();
    }
}
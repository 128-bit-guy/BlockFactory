using BlockFactory.Side_;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public class EntryPoint
{
    [SidedEntryPoint(Side.Client)]
    public static void ClientMain(string[] args)
    {
        new BlockFactoryClient().Run();
    }
}
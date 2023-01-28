using BlockFactory.Side_;

namespace BlockFactory.Server;

[ExclusiveTo(Side.Server)]
public class EntryPoint
{
    [SidedEntryPoint(Side.Server)]
    public static void ServerMain(string[] args)
    {
        BlockFactoryServer.Instance.Run();
    }
}
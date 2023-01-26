using BlockFactory.Client;
using BlockFactory.Server;
using BlockFactory.Side_;

namespace BlockFactory;

public class GameInstance
{
    public readonly Side PhysicalSide;
    [ExclusiveTo(Side.Client)] public BlockFactoryClient Client;
    public BlockFactoryServer? Server;

    public GameInstance(Side physicalSide)
    {
        PhysicalSide = physicalSide;
    }
}
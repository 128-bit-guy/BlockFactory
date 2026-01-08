using BlockFactory.Registry_;

namespace BlockFactory.Content.BlockInstance_;

public class BlockInstanceType : IRegistryEntry
{
    public int Id { get; set; }
    public Func<BlockInstance> Creator;

    public BlockInstanceType(Func<BlockInstance> creator)
    {
        Creator = creator;
    }

    public static BlockInstanceType Create<T>() where T : BlockInstance, new()
    {
        return new BlockInstanceType(() => new T());
    }
}
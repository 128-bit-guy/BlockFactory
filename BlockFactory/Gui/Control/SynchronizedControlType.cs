using BlockFactory.Registry_;

namespace BlockFactory.Gui.Control;

public class SynchronizedControlType : IRegistryEntry
{
    public int Id { get; set; }
    public Func<SynchronizedMenuControl> Creator;

    public SynchronizedControlType(Func<SynchronizedMenuControl> creator)
    {
        Creator = creator;
    }

    public static SynchronizedControlType Create<T>() where T : SynchronizedMenuControl, new()
    {
        return new SynchronizedControlType(() => new T());
    }
}
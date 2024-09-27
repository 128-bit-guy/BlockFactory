using BlockFactory.Registry_;

namespace BlockFactory.Content.Entity_;

public class EntityType : IRegistryEntry
{
    public int Id { get; set; }
    public Func<Entity> Creator;

    public EntityType(Func<Entity> creator)
    {
        Creator = creator;
    }

    public static EntityType Create<T>() where T : Entity, new()
    {
        return new EntityType(() => new T());
    }
}
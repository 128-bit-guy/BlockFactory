using BlockFactory.Registry_;

namespace BlockFactory.Entity_;

public class EntityType : RegistryEntry
{
    public Func<Entity> EntityCreator;

    public EntityType(Func<Entity> entityCreator)
    {
        EntityCreator = entityCreator;
    }

    public static EntityType Create<T>() where T : Entity, new()
    {
        return new EntityType(() => new T());
    }
}
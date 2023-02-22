using BlockFactory.Entity_;

namespace BlockFactory.World_.Api;

public interface IEntityStorage
{
    public void AddEntity(Entity entity, bool loaded = false);
    public void RemoveEntity(Entity entity);
}
using BlockFactory.Content.Entity_;

namespace BlockFactory.World_.Interfaces;

public interface IEntityStorage : IEntityAccess
{
    void AddEntity(Entity entity);
    void RemoveEntity(Entity entity);
}
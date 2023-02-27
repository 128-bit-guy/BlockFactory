using BlockFactory.Entity_;
using BlockFactory.Util.Math_;
using OpenTK.Mathematics;

namespace BlockFactory.World_.Api;

public interface IEntityStorage
{
    public void AddEntity(Entity entity, bool loaded = false);
    public void RemoveEntity(Entity entity);
    public IEnumerable<Entity> GetInBoxEntityEnumerable(EntityPos p, Box3 b);
}
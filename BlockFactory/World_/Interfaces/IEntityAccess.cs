using BlockFactory.Content.Entity_;
using Silk.NET.Maths;

namespace BlockFactory.World_.Interfaces;

public interface IEntityAccess
{
    IEnumerable<Entity> GetEntities(Box3D<double> box);
    Entity? GetEntity(Guid guid);
}
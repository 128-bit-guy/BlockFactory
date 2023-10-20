using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Entity_;

public class Entity
{
    public Vector3D<double> Pos;
    public World? World { get; private set; }

    public void SetWorld(World? world)
    {
        if (World != null)
        {
            OnRemovedFromWorld();
            World = null;
        }

        if (world != null)
        {
            World = world;
            OnAddedToWorld();
        }
    }

    public virtual void Update()
    {
    }

    protected virtual void OnRemovedFromWorld()
    {
    }

    protected virtual void OnAddedToWorld()
    {
    }
}
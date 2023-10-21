using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Math_;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D<int> GetBlockPos()
    {
        return new Vector3D<double>(Math.Floor(Pos.X), Math.Floor(Pos.Y), Math.Floor(Pos.Z)).As<int>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3D<int> GetChunkPos()
    {
        return GetBlockPos().ShiftRight(Constants.ChunkSizeLog2);
    }
}
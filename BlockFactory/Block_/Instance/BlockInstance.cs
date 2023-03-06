using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Serialization.Automatic;
using BlockFactory.Serialization.Automatic.Serializable;
using BlockFactory.World_;
using BlockFactory.World_.Chunk_;
using OpenTK.Mathematics;

namespace BlockFactory.Block_.Instance;

public class BlockInstance : AutoSerializable
{
    [NotSerialized] public GameInstance? GameInstance;
    public Vector3i Pos;
    [NotSerialized] public World? World;
    [NotSerialized] public Chunk? Chunk;

    public virtual void Tick()
    {
        Console.WriteLine($"Tick! {Pos}");
    }

    public virtual void OnAddToWorld()
    {
        
    }

    public virtual void OnRemoveFromWorld()
    {
        World = null;
    }

    public virtual bool OnUsed(PlayerEntity entity, (Vector3i pos, float time, Direction dir) rayCastRes)
    {
        return false;
    }
}
using System.Runtime.CompilerServices;
using BlockFactory.Base;
using BlockFactory.Serialization;
using BlockFactory.Utils;
using BlockFactory.Utils.Serialization;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_;

public abstract class Entity : ITagSerializable
{
    public Vector2D<float> HeadRotation;
    public Vector3D<double> Pos;
    [ExclusiveTo(Side.Client)] private DateTime _posSetTime;
    [ExclusiveTo(Side.Client)] private Vector3D<double> _prevPos;
    public Box3D<double> BoundingBox;
    public Guid Guid = Guid.NewGuid();
    public World? World { get; private set; }
    public Chunk? Chunk { get; private set; }
    public abstract EntityType Type { get; }

    public virtual DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        if (reason != SerializationReason.NetworkUpdate)
        {
            res.SetVector3D("pos", Pos);
            res.SetVector2D("head_rotation", HeadRotation);
            res.SetValue("guid", Guid.ToString());
        }

        return res;
    }
    
    [ExclusiveTo(Side.Client)]
    private Vector3D<double> GetInterpolatedPos()
    {
        var diff = DateTime.UtcNow - _posSetTime;
        var progress = diff.TotalMilliseconds / Constants.TickFrequencyMs / 2;
        progress = Math.Clamp(progress, 0, 1);
        return _prevPos * (1 - progress) + Pos * progress;
    }

    [ExclusiveTo(Side.Client)]
    public virtual Vector3D<double> GetSmoothPos()
    {
        return GetInterpolatedPos();
    }

    public void SetPos(Vector3D<double> pos)
    {
        Pos = pos;
        if (World != null && World!.LogicProcessor.LogicalSide != LogicalSide.Server)
        {
            _prevPos = GetInterpolatedPos();
            _posSetTime = DateTime.UtcNow;
        }
    }

    public virtual void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        if (reason != SerializationReason.NetworkUpdate)
        {
            Pos = tag.GetVector3D<double>("pos");
            HeadRotation = tag.GetVector2D<float>("head_rotation");
            Guid = Guid.Parse(tag.GetValue<string>("guid"));
        }
    }

    public void SetWorld(World? world, bool serialization)
    {
        if (World == world)
        {
            return;
        }
        if (World != null)
        {
            OnRemovedFromWorld(serialization);
            World = null;
        }

        if (world != null)
        {
            World = world;
            OnAddedToWorld(serialization);
        }
    }

    public void SetChunk(Chunk? c, bool serialization)
    {
        if (Chunk == c)
        {
            return;
        }

        if (Chunk != null)
        {
            OnRemovedFromChunk(serialization);
            Chunk = null;
        }

        if (c != null)
        {
            Chunk = c;
            OnAddedToChunk(serialization);
        }
    }

    public virtual void Update()
    {
    }

    protected virtual void OnRemovedFromWorld(bool serialization)
    {
    }

    protected virtual void OnAddedToWorld(bool serialization)
    {
    }

    protected virtual void OnRemovedFromChunk(bool serialization)
    {
        
    }

    protected virtual void OnAddedToChunk(bool serialization)
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

    public Vector3D<float> GetViewForward()
    {
        return new Vector3D<float>(MathF.Sin(HeadRotation.X) * MathF.Cos(HeadRotation.Y),
            MathF.Sin(HeadRotation.Y),
            MathF.Cos(HeadRotation.X) * MathF.Cos(HeadRotation.Y));
    }

    public Vector3D<float> GetViewUp()
    {
        return new Vector3D<float>(MathF.Sin(HeadRotation.X) * MathF.Cos(HeadRotation.Y + MathF.PI / 2),
            MathF.Sin(HeadRotation.Y + MathF.PI / 2),
            MathF.Cos(HeadRotation.X) * MathF.Cos(HeadRotation.Y + MathF.PI / 2));
    }

    public Vector3D<float> GetForward()
    {
        return new Vector3D<float>(MathF.Sin(HeadRotation.X), 0, MathF.Cos(HeadRotation.X));
    }

    public Vector3D<float> GetRight()
    {
        return new Vector3D<float>(-MathF.Cos(HeadRotation.X), 0, MathF.Sin(HeadRotation.X));
    }
}
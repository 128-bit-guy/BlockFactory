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
    public Guid Guid = Guid.NewGuid();
    public World? World { get; private set; }
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

    public virtual void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        if (reason != SerializationReason.NetworkUpdate)
        {
            Pos = tag.GetVector3D<double>("pos");
            HeadRotation = tag.GetVector2D<float>("head_rotation");
            Guid = Guid.Parse(tag.GetValue<string>("guid"));
        }
    }

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
using BlockFactory.Serialization;
using BlockFactory.Utils.Serialization;
using BlockFactory.World_;
using Silk.NET.Maths;

namespace BlockFactory.Content.BlockInstance_;

public abstract class BlockInstance : ITagSerializable
{
    public Vector3D<int> Pos;
    public Guid Guid = Guid.NewGuid();
    public World? World { get; private set; }
    public Chunk? Chunk { get; private set; }
    public abstract BlockInstanceType Type { get; }

    public void SetChunk(Chunk? c, bool serialization)
    {
        World = c?.World;
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

    protected virtual void OnRemovedFromChunk(bool serialization)
    {
        
    }

    protected virtual void OnAddedToChunk(bool serialization)
    {
        
    }

    public virtual DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        if (reason != SerializationReason.NetworkUpdate)
        {
            res.SetVector3D("pos", Pos);
            res.SetValue("guid", Guid.ToString());
        }

        return res;
    }

    public virtual void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        if (reason != SerializationReason.NetworkUpdate)
        {
            Pos = tag.GetVector3D<int>("pos");
            Guid = Guid.Parse(tag.GetValue<string>("guid"));
        }
    }
}
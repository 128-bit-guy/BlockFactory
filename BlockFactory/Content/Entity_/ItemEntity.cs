using BlockFactory.Content.Item_;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_;

public class ItemEntity : PhysicsEntity
{
    public override EntityType Type => Entities.Item;
    public ItemStack Stack;

    public ItemEntity(ItemStack stack)
    {
        Stack = stack;
        BoundingBox = new Box3D<double>(-0.4f, -0.4f, -0.4f, 0.4f, 0.4f, 0.4f);
    }

    public ItemEntity() : this(new ItemStack())
    {
        
    }

    public override void Update()
    {
        base.Update();
        if (Stack.Count == 0)
        {
            World!.RemoveEntity(this);
        }
    }

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var tag = base.SerializeToTag(reason);
        tag.Set("stack", Stack.SerializeToTag(reason));
        return tag;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        base.DeserializeFromTag(tag, reason);
        Stack = new ItemStack();
        Stack.DeserializeFromTag(tag.Get<DictionaryTag>("stack"), reason);
    }
}
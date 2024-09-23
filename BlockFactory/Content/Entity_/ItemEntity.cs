using BlockFactory.Content.Item_;
using BlockFactory.Serialization;
using Silk.NET.Maths;

namespace BlockFactory.Content.Entity_;

public class ItemEntity : PhysicsEntity
{
    public override EntityType Type => Entities.Item;
    public ItemStack Stack;
    public int PickUpDelay;

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
        if(World!.LogicProcessor.LogicalSide == LogicalSide.Client) return;
        if (Stack.Count == 0)
        {
            World!.RemoveEntity(this);
        }

        if (PickUpDelay > 0)
        {
            --PickUpDelay;
        }
    }

    public override DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var tag = base.SerializeToTag(reason);
        tag.Set("stack", Stack.SerializeToTag(reason));
        tag.SetValue("pick_up_delay", PickUpDelay);
        return tag;
    }

    public override void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        base.DeserializeFromTag(tag, reason);
        Stack = new ItemStack();
        Stack.DeserializeFromTag(tag.Get<DictionaryTag>("stack"), reason);
        PickUpDelay = tag.GetValue<int>("pick_up_delay");
    }
}
using BlockFactory.Content.Block_;
using BlockFactory.Serialization;

namespace BlockFactory.Content.Item_;

public class ItemStack : ITagSerializable, ICloneable
{
    public ItemInstance ItemInstance;
    public int Count;

    public ItemStack(ItemInstance itemInstance, int count)
    {
        ItemInstance = itemInstance;
        Count = count;
        SyncCountAndInstance();
    }
    
    public ItemStack(IItemProvider item, int count)
    {
        ItemInstance = item.AsItem().CreateInstance();
        Count = count;
        SyncCountAndInstance();
    }

    public ItemStack() : this(Blocks.Air, 0)
    {
        
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var res = new DictionaryTag();
        res.SetValue("item", ItemInstance.Item.Id);
        res.Set("item_instance", ItemInstance.SerializeToTag(reason));
        res.SetValue("count", Count);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        var item = Items.Registry[tag.GetValue<int>("item")]!;
        ItemInstance = item.CreateInstance();
        ItemInstance.DeserializeFromTag(tag.Get<DictionaryTag>("item_instance"), reason);
        Count = tag.GetValue<int>("count");
    }

    public void Increment()
    {
        ++Count;
        SyncCountAndInstance();
    }

    public void Decrement()
    {
        --Count;
        SyncCountAndInstance();
    }

    public void SyncCountAndInstance()
    {
        if (Count <= 0)
        {
            ItemInstance = Blocks.Air.AsItem().CreateInstance();
        }
        
        if (ItemInstance.IsEmpty())
        {
            Count = 0;
        }
    }


    public object Clone()
    {
        var newInstance = (ItemInstance)ItemInstance.Clone();
        return new ItemStack(newInstance, Count);
    }
}
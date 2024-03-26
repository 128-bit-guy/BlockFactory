using BlockFactory.Block_;
using BlockFactory.Serialization;

namespace BlockFactory.Item_;

public class ItemStack : ITagSerializable
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
}
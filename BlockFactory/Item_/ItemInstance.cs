using BlockFactory.Block_;
using BlockFactory.Serialization;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace BlockFactory.Item_;

public class ItemInstance : ITagSerializable, IEquatable<ItemInstance>, ICloneable
{
    public readonly Item Item;

    public ItemInstance(Item item)
    {
        Item = item;
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        return new DictionaryTag();
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
    }

    public object Clone()
    {
        return Item.CreateInstance();
    }

    public bool Equals(ItemInstance? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return ReferenceEquals(Item, other.Item);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ItemInstance)obj);
    }

    public override int GetHashCode()
    {
        return Item.Id;
    }

    public bool IsEmpty()
    {
        return Item == Blocks.Air.AsItem();
    }

    public static bool operator ==(ItemInstance? left, ItemInstance? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ItemInstance? left, ItemInstance? right)
    {
        return !Equals(left, right);
    }
}
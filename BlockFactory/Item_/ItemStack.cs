﻿using BlockFactory.Init;

namespace BlockFactory.Item_;

public class ItemStack : IEquatable<ItemStack>
{
    public static readonly ItemStack Empty = new ItemStack(Items.BlockItems[Blocks.Air], 0);
    public readonly Item Item;
    public readonly int Count;

    public ItemStack(Item item, int count)
    {
        if (count == 0 || item == Items.BlockItems[Blocks.Air])
        {
            Item = Items.BlockItems[Blocks.Air];
            Count = 0;
        }
        else
        {
            Item = item;
            Count = count;
        }
    }

    public ItemStack(BinaryReader reader)
    {
        Item = Items.Registry[reader.Read7BitEncodedInt()];
        Count = reader.Read7BitEncodedInt();
    }
    
    public ItemStack(Item item) : this(item, 1)
    {
    }

    public static ItemStack operator +(ItemStack stack, int count)
    {
        if (stack.Count + count == 0 || stack.Item == Items.BlockItems[Blocks.Air])
        {
            return Empty;
        }
        else
        {
            return new ItemStack(stack.Item, stack.Count + count);
        }
    }

    public static ItemStack operator -(ItemStack stack, int count)
    {
        return stack + (-count);
    }

    public bool Equals(ItemStack? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Item == other.Item && Count == other.Count;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((ItemStack)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Item.Id, Count);
    }

    public static bool operator ==(ItemStack? left, ItemStack? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ItemStack? left, ItemStack? right)
    {
        return !Equals(left, right);
    }

    public bool IsEmpty()
    {
        return Count == 0;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(Item.Id);
        writer.Write7BitEncodedInt(Count);
    }

    public bool CanMergeWith(ItemStack other)
    {
        if (IsEmpty() || other.IsEmpty())
        {
            return true;
        }
        else
        {
            return Item == other.Item;
        }
    }

    public ItemStack WithCount(int nCnt)
    {
        return new ItemStack(Item, nCnt);
    }
}
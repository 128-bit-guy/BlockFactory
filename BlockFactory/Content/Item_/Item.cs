using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Registry_;
using BlockFactory.World_;

namespace BlockFactory.Content.Item_;

public class Item : IRegistryEntry, IItemProvider
{
    public int Id { get; set; }
    private readonly ItemInstance _defaultInstance;

    public Item()
    {
        _defaultInstance = new ItemInstance(this);
    }

    public Item AsItem()
    {
        return this;
    }

    public ItemInstance CreateInstance()
    {
        return _defaultInstance;
    }

    public virtual void Use(ItemStack stack, BlockPointer pointer, CubeFace face, object user)
    {
        
    }

    public virtual int GetMaxCount(ItemInstance instance)
    {
        if (IsNonStackable())
        {
            return 1;
        }
        else
        {
            return 64;
        }
    }

    public virtual bool IsNonStackable()
    {
        return false;
    }

    [ExclusiveTo(Side.Client)]
    public virtual int GetTexture(ItemStack stack)
    {
        return 0;
    }
}
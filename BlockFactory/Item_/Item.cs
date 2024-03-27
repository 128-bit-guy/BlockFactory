using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Entity_;
using BlockFactory.Registry_;
using BlockFactory.World_;

namespace BlockFactory.Item_;

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

    [ExclusiveTo(Side.Client)]
    public virtual int GetTexture(ItemStack stack)
    {
        return 0;
    }
}
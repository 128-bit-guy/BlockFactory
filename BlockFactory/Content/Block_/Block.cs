using BlockFactory.Base;
using BlockFactory.Content.Item_;
using BlockFactory.CubeMath;
using BlockFactory.Registry_;
using BlockFactory.World_;
using BlockFactory.World_.Light;
using Silk.NET.Maths;

namespace BlockFactory.Content.Block_;

public class Block : IRegistryEntry, IItemProvider
{
    public int Id { get; set; }

    [ExclusiveTo(Side.Client)]
    public virtual bool BlockRendering(CubeFace face)
    {
        return true;
    }

    [ExclusiveTo(Side.Client)]
    public virtual int GetTexture(CubeFace face)
    {
        return 0;
    }

    public virtual Block GetWorldGenBase()
    {
        return this;
    }

    public virtual byte GetEmittedLight()
    {
        return 0;
    }

    public virtual bool CanLightEnter(CubeFace face, LightChannel channel)
    {
        return false;
    }

    public virtual bool CanLightLeave(CubeFace face, LightChannel channel)
    {
        return true;
    }

    [ExclusiveTo(Side.Client)]
    public virtual bool HasAo()
    {
        return true;
    }

    public virtual void UpdateBlock(BlockPointer pointer)
    {
    }

    public virtual bool IsFaceSolid(CubeFace face)
    {
        return true;
    }

    public virtual void RandomUpdateBlock(BlockPointer pointer)
    {
    }

    public Item AsItem()
    {
        return Items.BlockItems[this];
    }

    public List<ItemStack> GetDroppedStacks(BlockPointer pointer)
    {
        return new List<ItemStack>{new ItemStack(this, 1)};
    }

    public virtual bool HasCollision()
    {
        return true;
    }

    public virtual bool IsReplaceable()
    {
        return false;
    }

    public virtual BlockLightType GetLightType()
    {
        return BlockLightType.Blocking;
    }
}
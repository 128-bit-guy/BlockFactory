using BlockFactory.Base;
using BlockFactory.CubeMath;
using BlockFactory.Registry_;
using BlockFactory.World_.Light;

namespace BlockFactory.Block_;

public class Block : IRegistryEntry
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

    public virtual byte GetEmittedLight(LightChannel channel)
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
}
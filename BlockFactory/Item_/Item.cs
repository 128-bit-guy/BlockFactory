using BlockFactory.CubeMath;
using BlockFactory.Entity_.Player;
using BlockFactory.Inventory_;
using BlockFactory.Registry_;
using OpenTK.Mathematics;

namespace BlockFactory.Item_;

public class Item : IRegistryEntry
{
    public int Id { get; set; }

    public virtual bool OnUse(SlotPointer container, PlayerEntity entity,
        (Vector3i pos, float time, Direction dir)? rayCastRes)
    {
        return false;
    }

    public virtual int GetMaxStackSize(ItemStack stack)
    {
        return 64;
    }
}
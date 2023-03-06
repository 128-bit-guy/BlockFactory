﻿using BlockFactory.Game;
using BlockFactory.Init;
using BlockFactory.Item_;
using OpenTK.Mathematics;

namespace BlockFactory.Entity_;

public class ItemEntity : PhysicsEntity
{
    public ItemStack Stack = ItemStack.Empty;
    public int PickUpDelay = 20;

    public ItemEntity()
    {
    }

    public ItemEntity(ItemStack stack)
    {
        Stack = stack;
    }

    public override EntityType Type => Entities.Item;
    public override Box3 GetBoundingBox()
    {
        return new Box3(new Vector3(-0.2f), new Vector3(0.2f));
    }

    public override int MaxHealth => 1;

    protected override void TickInternal()
    {
        base.TickInternal();
        if (World!.GameInstance.Kind.DoesProcessLogic() && PickUpDelay > 0)
        {
            --PickUpDelay;
        }
    }
}
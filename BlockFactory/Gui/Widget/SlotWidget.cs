﻿using BlockFactory.Inventory_;
using BlockFactory.Item_;
using OpenTK.Mathematics;

namespace BlockFactory.Gui.Widget;

public class SlotWidget : InGameMenuWidget
{
    public readonly SlotGroup Group;
    public readonly IInventory? Inv;
    public readonly int Slot;
    public ItemStack Stack;

    public SlotWidget(InGameMenuWidgetType type, Vector2i pos, IInventory inv, int slot, SlotGroup? group = null) :
        base(type,
            new Box2i(pos, pos))
    {
        Inv = inv;
        Slot = slot;
        Stack = ItemStack.Empty;
        if (group == null)
            Group = new SlotGroup();
        else
            Group = group;

        Group.Slots.Add(this);
    }

    public SlotWidget(InGameMenuWidgetType type, BinaryReader reader) : base(type, reader)
    {
        Inv = null;
        Slot = -1;
        Stack = ItemStack.Empty;
        Group = new SlotGroup();
    }

    public override void Update()
    {
        base.Update();
        if (Inv != null && Stack != Inv[Slot])
        {
            Stack = Inv[Slot];
            SendUpdate();
        }
    }

    public override void WriteUpdateData(BinaryWriter writer)
    {
        base.WriteUpdateData(writer);
        Stack.Write(writer);
    }

    public override void ReadUpdateData(BinaryReader reader)
    {
        base.ReadUpdateData(reader);
        Stack = new ItemStack(reader);
    }

    protected virtual void PickFullStack()
    {
        var stack = Inv!.TryExtractStack(Slot, Inv[Slot].Count, true);
        var excess = Menu.MovedStackInventory.TryInsertStack(0, stack, true);
        var movedCount = stack.Count - excess.Count;
        var resStack = Inv.TryExtractStack(Slot, movedCount, false);
        Menu.MovedStackInventory.TryInsertStack(0, resStack, false);
    }

    protected virtual void PutFullStack()
    {
        var stack = Menu.MovedStackInventory.TryExtractStack(0,
            Menu.MovedStackInventory[0].Count, true);
        var excess = Inv!.TryInsertStack(Slot, stack, true);
        var movedCount = stack.Count - excess.Count;
        var resStack = Menu.MovedStackInventory.TryExtractStack(0, movedCount, false);
        Inv.TryInsertStack(Slot, resStack, false);
    }

    protected virtual void PickHalfStack()
    {
        var stack = Inv!.TryExtractStack(Slot, Inv[Slot].Count, true);
        var excess = Menu.MovedStackInventory.TryInsertStack(0, stack, true);
        var movedCount = (stack.Count - excess.Count + 1) / 2;
        var resStack = Inv.TryExtractStack(Slot, movedCount, false);
        Menu.MovedStackInventory.TryInsertStack(0, resStack, false);
    }

    protected virtual void PutOneItem()
    {
        var stack = Menu.MovedStackInventory.TryExtractStack(0, 1, true);
        var excess = Inv!.TryInsertStack(Slot, stack, true);
        var movedCount = stack.Count - excess.Count;
        var resStack = Menu.MovedStackInventory.TryExtractStack(0, movedCount, false);
        Inv.TryInsertStack(Slot, resStack, false);
    }

    private ItemStack PutIntoNextGroup(ItemStack stack, bool simulate)
    {
        if (Group.Next == null)
        {
            return stack;
        }

        var left = stack;
        foreach (var slot in Group.Next.Slots) left = slot.Inv!.TryInsertStack(slot.Slot, left, simulate);

        return left;
    }

    protected virtual void MoveStack()
    {
        var stack = Inv!.TryExtractStack(Slot, Inv[Slot].Count, true);
        var excess = PutIntoNextGroup(stack, true);
        var delta = stack.Count - excess.Count;
        var resStack = Inv!.TryExtractStack(Slot, delta, false);
        PutIntoNextGroup(resStack, false);
    }

    protected virtual void SwapStacks()
    {
        var invStack = Inv!.TryExtractStack(Slot, Inv[Slot].Count, true);
        if (invStack.Count != Inv[Slot].Count) return;

        var movedStack = Menu.MovedStackInventory.TryExtractStack(0,
            Menu.MovedStackInventory[0].Count, true);
        if (movedStack.Count != Menu.MovedStackInventory[0].Count) return;

        var invExcess = Menu.MovedStackInventory.GetExcessIfSlotIsEmpty(0, invStack);
        if (!invExcess.IsEmpty()) return;

        var movedExcess = Inv!.GetExcessIfSlotIsEmpty(Slot, movedStack);

        if (!movedExcess.IsEmpty()) return;

        var resInvStack = Inv!.TryExtractStack(Slot, Inv[Slot].Count, false);

        var resMovedStack = Menu.MovedStackInventory.TryExtractStack(0,
            Menu.MovedStackInventory[0].Count, false);

        Menu.MovedStackInventory.TryInsertStack(0, resInvStack, false);

        Inv!.TryInsertStack(Slot, resMovedStack, false);
    }

    protected override void OnAction(int actionNumber)
    {
        base.OnAction(actionNumber);
        if (actionNumber == 2 || actionNumber == 3)
        {
            MoveStack();
        }
        else
        {
            if (Menu.MovedStackInventory[0].IsEmpty())
                switch (actionNumber)
                {
                    case 0:
                        PickFullStack();

                        break;
                    case 1:
                        PickHalfStack();

                        break;
                }
            else if (Inv![Slot].CanMergeWith(Menu.MovedStackInventory[0]))
                switch (actionNumber)
                {
                    case 0:
                        PutFullStack();
                        break;
                    case 1:
                        PutOneItem();
                        break;
                }
            else
                SwapStacks();
        }
    }
}
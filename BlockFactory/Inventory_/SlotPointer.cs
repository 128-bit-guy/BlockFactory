using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public struct SlotPointer
{
    public IInventory Inv;
    public int Slot;

    public SlotPointer(IInventory inv, int slot)
    {
        Inv = inv;
        Slot = slot;
    }

    public ItemStack GetStack()
    {
        return Inv[Slot];
    }
    
    public ItemStack TryInsertStack(ItemStack stack, bool simulate)
    {
        return Inv.TryInsertStack(Slot, stack, simulate);
    }

    public ItemStack TryExtractStack(int count, bool simulate)
    {
        return Inv.TryExtractStack(Slot, count, simulate);
    }

    public ItemStack GetExcessIfSlotIsEmpty(ItemStack stack)
    {
        return Inv.GetExcessIfSlotIsEmpty(Slot, stack);
    }
}
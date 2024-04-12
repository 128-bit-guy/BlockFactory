namespace BlockFactory.Content.Item_.Inventory_;

public static class InventoryUtils
{
    public static int Transfer(IInventory from, int fromSlot, IInventory to, int toSlot, int maxCnt = int.MaxValue)
    {
        var extractable = from.Extract(fromSlot, maxCnt, true);
        var excess = to.Insert(toSlot, extractable, true);
        var res = extractable.Count - excess.Count;
        to.Insert(toSlot, from.Extract(fromSlot, res, false), false);
        return res;
    }

    public static bool Swap(IInventory from, int fromSlot, IInventory to, int toSlot)
    {
        if (!from.IsWhollyAcceptable(fromSlot, to[toSlot]) || !to.IsWhollyAcceptable(toSlot, from[fromSlot]))
        {
            return false;
        }

        var fromStack = from.Extract(fromSlot, int.MaxValue, false);
        var toStack = to.Extract(toSlot, int.MaxValue, false);
        to.Insert(toSlot, fromStack, false);
        from.Insert(fromSlot, toStack, false);
        
        return true;
    }

    public static ItemStack Insert(IInventory inventory, ItemStack stack, bool simulate)
    {
        var curStack = (ItemStack)stack.Clone();
        for (var i = 0; i < inventory.SlotCount; ++i)
        {
            curStack = inventory.Insert(i, curStack, simulate);
        }

        return curStack;
    }
}
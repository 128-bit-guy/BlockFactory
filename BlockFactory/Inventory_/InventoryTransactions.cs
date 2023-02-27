using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public static class InventoryTransactions
{
    public static ItemStack Insert(this IInventory inventory, ItemStack stack, bool simulate)
    {
        var left = stack;
        for (int i = 0; i < inventory.Size; ++i)
        {
            left = inventory.TryInsertStack(i, left, simulate);
        }

        return left;
    }

    public static ItemStack Extract(this IInventory inventory, int cnt, Predicate<ItemStack> p, bool simulate)
    {
        var res = ItemStack.Empty;
        for (var i = 0; i < inventory.Size; ++i)
        {
            if (!p(inventory[i]) || !res.CanMergeWith(inventory[i])) continue;
            var s = inventory.TryExtractStack(i, cnt - res.Count, simulate);
            if (!s.IsEmpty())
            {
                res = s.Incremented(res.Count);
            }
        }

        return res;
    }
}
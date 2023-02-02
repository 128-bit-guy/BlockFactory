using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public class SimpleInventory : IInventory
{
    private readonly ItemStack[] Stacks;

    public SimpleInventory(ItemStack[] stacks)
    {
        Stacks = stacks;
        OnSlotContentChanged = (_, _, _) => { };
    }

    public SimpleInventory(int count) : this(new ItemStack[count])
    {
        Array.Fill(Stacks, ItemStack.Empty);
    }

    public int Size => Stacks.Length;

    public ItemStack this[int i]
    {
        get => Stacks[i];
        set
        {
            var previous = Stacks[i];
            Stacks[i] = value;
            OnSlotContentChanged(i, previous, Stacks[i]);
        }
    }

    public ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate)
    {
        if (stack.IsEmpty()) return ItemStack.Empty;

        if (stack.CanMergeWith(Stacks[slot]))
        {
            var maxCnt = stack.Item.GetMaxStackSize(stack);
            var newCnt = Math.Min(Stacks[slot].Count + stack.Count, maxCnt);
            var delta = newCnt - Stacks[slot].Count;
            if (!simulate)
            {
                var previous = Stacks[slot];
                if (Stacks[slot].IsEmpty())
                    Stacks[slot] = stack.WithCount(newCnt);
                else
                    Stacks[slot] += delta;

                OnSlotContentChanged(slot, previous, Stacks[slot]);
            }

            return stack - delta;
        }

        return stack;
    }

    public ItemStack TryExtractStack(int slot, int count, bool simulate)
    {
        var delta = Math.Min(Stacks[slot].Count, count);
        var res = Stacks[slot].WithCount(delta);
        if (!simulate)
        {
            var previous = Stacks[slot];
            Stacks[slot] -= delta;
            OnSlotContentChanged(slot, previous, Stacks[slot]);
        }

        return res;
    }

    public ItemStack GetExcessIfSlotIsEmpty(int slot, ItemStack stack)
    {
        var max = stack.Item.GetMaxStackSize(stack);
        var delta = stack.Count - Math.Min(max, stack.Count);
        return stack.WithCount(delta);
    }

    public event IInventory.SlotContentChangeHandler OnSlotContentChanged;
}
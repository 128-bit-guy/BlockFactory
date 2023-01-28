using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public class SimpleInventory : IInventory
{
    private ItemStack[] Stacks;

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
            ItemStack previous = Stacks[i];
            Stacks[i] = value;
            OnSlotContentChanged(i, previous, Stacks[i]);
        }
    }

    public ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate)
    {
        if (stack.IsEmpty())
        {
            return ItemStack.Empty;
        }
        else if(stack.CanMergeWith(Stacks[slot]))
        {
            int maxCnt = stack.Item.GetMaxStackSize(stack);
            int newCnt = Math.Min(Stacks[slot].Count + stack.Count, maxCnt);
            int delta = newCnt - Stacks[slot].Count;
            if (!simulate)
            {
                ItemStack previous = Stacks[slot];
                if (Stacks[slot].IsEmpty())
                {
                    Stacks[slot] = stack.WithCount(newCnt);
                }
                else
                {
                    Stacks[slot] += delta;
                }

                OnSlotContentChanged(slot, previous, Stacks[slot]);
            }
            return stack - delta;
        }
        else
        {
            return stack;
        }
    }

    public ItemStack TryExtractStack(int slot, int count, bool simulate)
    {
        int delta = Math.Min(Stacks[slot].Count, count);
        ItemStack res = Stacks[slot].WithCount(delta);
        if (!simulate)
        {
            ItemStack previous = Stacks[slot];
            Stacks[slot] -= delta;
            OnSlotContentChanged(slot, previous, Stacks[slot]);
        }
        return res;
    }

    public ItemStack GetExcessIfSlotIsEmpty(int slot, ItemStack stack)
    {
        int max = stack.Item.GetMaxStackSize(stack);
        int delta = stack.Count - Math.Min(max, stack.Count);
        return stack.WithCount(delta);
    }

    public event IInventory.SlotContentChangeHandler OnSlotContentChanged;
}
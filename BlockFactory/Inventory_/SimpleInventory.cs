using BlockFactory.Item_;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Inventory_;

public class SimpleInventory : IInventory, ITagSerializable
{
    private ItemStack[] _stacks;

    public SimpleInventory(ItemStack[] stacks)
    {
        _stacks = stacks;
        OnSlotContentChanged = (_, _, _) => { };
    }

    public SimpleInventory(int count) : this(new ItemStack[count])
    {
        Array.Fill(_stacks, ItemStack.Empty);
    }

    public int Size => _stacks.Length;

    public ItemStack this[int i]
    {
        get => _stacks[i];
        set
        {
            var previous = _stacks[i];
            _stacks[i] = value;
            OnSlotContentChanged(i, previous, _stacks[i]);
        }
    }

    public ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate)
    {
        if (stack.IsEmpty()) return ItemStack.Empty;

        if (stack.CanMergeWith(_stacks[slot]))
        {
            var maxCnt = stack.Item.GetMaxStackSize(stack);
            var newCnt = Math.Min(_stacks[slot].Count + stack.Count, maxCnt);
            var delta = newCnt - _stacks[slot].Count;
            if (!simulate)
            {
                var previous = _stacks[slot];
                if (_stacks[slot].IsEmpty())
                    _stacks[slot] = stack.WithCount(newCnt);
                else
                    _stacks[slot] += delta;

                OnSlotContentChanged(slot, previous, _stacks[slot]);
            }

            return stack - delta;
        }

        return stack;
    }

    public ItemStack TryExtractStack(int slot, int count, bool simulate)
    {
        var delta = Math.Min(_stacks[slot].Count, count);
        var res = _stacks[slot].WithCount(delta);
        if (!simulate)
        {
            var previous = _stacks[slot];
            _stacks[slot] -= delta;
            OnSlotContentChanged(slot, previous, _stacks[slot]);
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
    public DictionaryTag SerializeToTag()
    {
        var res = new DictionaryTag();
        var tag = new ListTag(_stacks.Length, TagType.Dictionary);
        for (var i = 0; i < _stacks.Length; ++i)
        {
            tag.Set(i, _stacks[i].SerializeToTag());
        }
        res.Set("Stacks", tag);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        var list = tag.Get<ListTag>("Stacks");
        _stacks = new ItemStack[list.Count];
        for (var i = 0; i < list.Count; ++i)
        {
            _stacks[i] = new ItemStack(list.Get<DictionaryTag>(i));
        }
    }
}
using BlockFactory.Content.Block_;
using BlockFactory.Serialization;

namespace BlockFactory.Content.Item_.Inventory_;

public class Inventory : IInventory, ITagSerializable
{
    private readonly ItemStack[] _slots;

    public Inventory(int cnt)
    {
        _slots = new ItemStack[cnt];
        for (int i = 0; i < cnt; ++i)
        {
            _slots[i] = new ItemStack(Blocks.Air, 0);
        }
    }

    public ItemStack this[int x] => _slots[x];

    public int SlotCount => _slots.Length;
    
    public ItemStack Insert(int slot, ItemStack stack, bool simulate)
    {
        if (stack.ItemInstance.IsEmpty())
        {
            return stack;
        }

        var c = (ItemStack)stack.Clone();
        c.Count = 1;
        if (!IsWhollyAcceptable(slot, c))
        {
            return stack;
        }

        if (!ItemUtils.CanStack(_slots[slot], stack))
        {
            return stack;
        }

        var maxCnt = stack.ItemInstance.Item.GetMaxCount(stack.ItemInstance);
        var transferCount = Math.Min(maxCnt - _slots[slot].Count, stack.Count);
        if (!simulate)
        {
            if (_slots[slot].ItemInstance.IsEmpty())
            {
                _slots[slot].ItemInstance = (ItemInstance)stack.ItemInstance.Clone();
            }
            _slots[slot].Count += transferCount;
            _slots[slot].SyncCountAndInstance();
        }

        var leftStack = (ItemStack)stack.Clone();
        leftStack.Count -= transferCount;
        leftStack.SyncCountAndInstance();
        return leftStack;
    }

    public ItemStack Extract(int slot, int maxCnt, bool simulate)
    {
        var res = (ItemStack)_slots[slot].Clone();
        res.Count = Math.Min(res.Count, maxCnt);
        if (!simulate)
        {
            _slots[slot].Count -= res.Count;
            _slots[slot].SyncCountAndInstance();
        }

        return res;
    }

    public virtual bool IsWhollyAcceptable(int slot, ItemStack stack)
    {
        return true;
    }

    public DictionaryTag SerializeToTag(SerializationReason reason)
    {
        var list = new ListTag();
        foreach (var stack in _slots)
        {
            list.Add(stack.SerializeToTag(reason));
        }

        var res = new DictionaryTag();
        res.Set("slots", list);
        return res;
    }

    public void DeserializeFromTag(DictionaryTag tag, SerializationReason reason)
    {
        var list = tag.Get<ListTag>("slots");
        int pos = 0;
        foreach (var itemTag in list.GetEnumerable<DictionaryTag>())
        {
            if(pos >= _slots.Length) break;
            _slots[pos] = new ItemStack();
            _slots[pos++].DeserializeFromTag(itemTag, reason);
        }

        for (; pos < _slots.Length; ++pos)
        {
            _slots[pos] = new ItemStack();
        }
    }
}
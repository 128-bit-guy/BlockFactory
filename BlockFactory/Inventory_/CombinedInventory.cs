using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public class CombinedInventory : IInventory
{
    private IInventory[] _inventories;

    public CombinedInventory(params IInventory[] inventories)
    {
        _inventories = inventories;
    }

    public int Size => _inventories.Select(inv => inv.Size).Sum();

    public ItemStack this[int i]
    {
        get
        {
            for (int j = 0; j < _inventories.Length; ++j)
            {
                if (_inventories[j].Size > i)
                {
                    return _inventories[j][i];
                }

                i -= _inventories[j].Size;
            }

            throw new IndexOutOfRangeException();
        }
}

    public ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate)
    {
        for (int j = 0; j < _inventories.Length; ++j)
        {
            if (_inventories[j].Size > slot)
            {
                return _inventories[j].TryInsertStack(slot, stack, simulate);
            }

            slot -= _inventories[j].Size;
        }

        throw new IndexOutOfRangeException();
    }

    public ItemStack TryExtractStack(int slot, int count, bool simulate)
    {
        for (int j = 0; j < _inventories.Length; ++j)
        {
            if (_inventories[j].Size > slot)
            {
                return _inventories[j].TryExtractStack(slot, count, simulate);
            }

            slot -= _inventories[j].Size;
        }

        throw new IndexOutOfRangeException();
    }

    public ItemStack GetExcessIfSlotIsEmpty(int slot, ItemStack stack)
    {
        for (int j = 0; j < _inventories.Length; ++j)
        {
            if (_inventories[j].Size > slot)
            {
                return _inventories[j].GetExcessIfSlotIsEmpty(slot, stack);
            }

            slot -= _inventories[j].Size;
        }

        throw new IndexOutOfRangeException();
    }

    public event IInventory.SlotContentChangeHandler? OnSlotContentChanged
    {
        add { throw new NotImplementedException(); }
        remove { throw new NotImplementedException(); }
    }
}
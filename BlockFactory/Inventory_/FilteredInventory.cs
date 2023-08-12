using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public class FilteredInventory : IInventory
{
    public delegate bool Filter(int slot, ItemStack stack);
    private IInventory _inventory;
    private Filter _filter;
    public FilteredInventory(IInventory inventory, Filter filter)
    {
        _inventory = inventory;
        _filter = filter;
    }

    public int Size => _inventory.Size;

    public ItemStack this[int i] => _inventory[i];

    public ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate)
    {
        if (_filter(slot, stack))
        {
            return _inventory.TryInsertStack(slot, stack, simulate);
        }

        return stack;
    }

    public ItemStack TryExtractStack(int slot, int count, bool simulate)
    {
        return _inventory.TryExtractStack(slot, count, simulate);
    }

    public ItemStack GetExcessIfSlotIsEmpty(int slot, ItemStack stack)
    {
        if (_filter(slot, stack))
        {
            return _inventory.GetExcessIfSlotIsEmpty(slot, stack);
        }

        return stack;
    }

    public event IInventory.SlotContentChangeHandler OnSlotContentChanged
    {
        add => _inventory.OnSlotContentChanged += value;
        remove => _inventory.OnSlotContentChanged -= value;
    }
}
using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public interface IInventory
{
    int Size { get; }
    
    ItemStack this[int i] { get; }

    ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate);

    ItemStack TryExtractStack(int slot, int count, bool simulate);

    ItemStack GetExcessIfSlotIsEmpty(int slot, ItemStack stack);

    delegate void SlotContentChangeHandler(int slot, ItemStack previous, ItemStack current);

    event SlotContentChangeHandler OnSlotContentChanged;
}
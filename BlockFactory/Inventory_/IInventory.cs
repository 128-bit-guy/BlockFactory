using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public interface IInventory
{
    delegate void SlotContentChangeHandler(int slot, ItemStack previous, ItemStack current);

    int Size { get; }

    ItemStack this[int i] { get; }

    ItemStack TryInsertStack(int slot, ItemStack stack, bool simulate);

    ItemStack TryExtractStack(int slot, int count, bool simulate);

    ItemStack GetExcessIfSlotIsEmpty(int slot, ItemStack stack);

    event SlotContentChangeHandler OnSlotContentChanged;
}
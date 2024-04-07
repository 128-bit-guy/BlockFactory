namespace BlockFactory.Item_.Inventory_;

public interface IInventory
{
    ItemStack this[int x] { get; }
    int SlotCount { get; }
    ItemStack Insert(int slot, ItemStack stack, bool simulate);
    ItemStack Extract(int slot, int maxCnt, bool simulate);
    bool IsWhollyAcceptable(int slot, ItemStack stack);
}
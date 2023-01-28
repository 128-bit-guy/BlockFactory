using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public interface IStackContainer
{
    ItemStack GetStack();
    void ChangeStack(ItemStack stack);
}
using BlockFactory.Item_;

namespace BlockFactory.Inventory_;

public class SimpleStackContainer : IStackContainer
{
    public ItemStack Stack;

    public SimpleStackContainer(ItemStack stack)
    {
        Stack = stack;
    }

    public ItemStack GetStack()
    {
        return Stack;
    }

    public void ChangeStack(ItemStack stack)
    {
        Stack = stack;
    }
}
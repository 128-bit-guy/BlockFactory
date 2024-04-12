namespace BlockFactory.Content.Item_;

public static class ItemUtils
{
    public static bool CanStack(ItemStack a, ItemStack b)
    {
        if (a.ItemInstance.IsEmpty() || b.ItemInstance.IsEmpty())
        {
            return true;
        }

        return a.ItemInstance.Equals(b.ItemInstance);
    }
}
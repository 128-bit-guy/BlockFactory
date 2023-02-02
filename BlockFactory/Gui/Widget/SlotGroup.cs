namespace BlockFactory.Gui.Widget;

public class SlotGroup
{
    public SlotGroup? Next;
    public List<SlotWidget> Slots;

    public SlotGroup()
    {
        Slots = new List<SlotWidget>();
    }
}
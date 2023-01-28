namespace BlockFactory.Gui.Widget;

public class SlotGroup
{
    public List<SlotWidget> Slots;
    public SlotGroup? Next;

    public SlotGroup()
    {
        Slots = new List<SlotWidget>();
    }
}
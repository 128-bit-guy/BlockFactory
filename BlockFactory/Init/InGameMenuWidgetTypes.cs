using BlockFactory.Gui.Widget;
using BlockFactory.Registry_;
using BlockFactory.Gui;

namespace BlockFactory.Init;

public class InGameMenuWidgetTypes
{
    public static Registry<InGameMenuWidgetType> Registry { get; private set; } = null!;
    public static InGameMenuWidgetType Slot;
    public static InGameMenuWidgetType Label;

    public static void Init()
    {
        Registry = SyncedRegistries.NewSyncedRegistry<InGameMenuWidgetType>(new RegistryName("InGameWidgetTypes"));
        Slot = Registry.Register(new RegistryName("Slot"),
            new InGameMenuWidgetType((t, r) => new SlotWidget(t, r)));
        Label = Registry.Register(new RegistryName("Label"),
            new InGameMenuWidgetType((t, r) => new LabelWidget(t, r)));
        Registry.Lock();
    }
}
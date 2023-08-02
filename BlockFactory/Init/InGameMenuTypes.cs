using BlockFactory.Gui;
using BlockFactory.Registry_;

namespace BlockFactory.Init;

public class InGameMenuTypes
{
    public static Registry<InGameMenuType> Registry;
    public static InGameMenuType PlayerInventory;
    public static InGameMenuType Workbench;

    public static void Init()
    {
        Registry = SyncedRegistries.NewSyncedRegistry<InGameMenuType>(new RegistryName("InGameMenuType"));
        PlayerInventory = Registry.Register(new RegistryName("PlayerInventory"),
            new InGameMenuType((t, r) => new PlayerInventoryMenu(t, r)));
        Workbench = new InGameMenuType((t, r) => new WorkbenchMenu(t, r));
        Registry.Lock();
    }
}
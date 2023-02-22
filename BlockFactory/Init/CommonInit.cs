using BlockFactory.Entity_.Player;

namespace BlockFactory.Init;

public static class CommonInit
{
    public static void Init()
    {
        Packets.Init();
        SyncedRegistries.Init();
        Blocks.Init();
        Items.Init();
        Entities.Init();
        InGameMenuWidgetTypes.Init();
        InGameMenuTypes.Init();
        PlayerChunkLoading.Init();
        Serializers.Init();
    }
}
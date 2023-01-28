using BlockFactory.Entity_.Player;
using BlockFactory.Entity_;

namespace BlockFactory.Init
{
    public static class CommonInit
    {
        public static void Init()
        {
            Packets.Init();
            SyncedRegistries.Init();
            Blocks.Init();
            Items.Init();
            InGameMenuWidgetTypes.Init();
            InGameMenuTypes.Init();
            PlayerChunkLoading.Init();
        }
    }
}
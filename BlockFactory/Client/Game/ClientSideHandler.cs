using BlockFactory.Client.Entity_;
using BlockFactory.Entity_;
using BlockFactory.Game;
using BlockFactory.Util.Math_;

namespace BlockFactory.Client.Game
{
    public class ClientSideHandler : ISideHandler
    {
        private readonly BlockFactoryClient _client;

        public ClientSideHandler(BlockFactoryClient client)
        {
            _client = client;
        }

        public void SetEntityPos(Entity entity, EntityPos pos)
        {
            entity.SetNewPos(pos);
        }
    }
}

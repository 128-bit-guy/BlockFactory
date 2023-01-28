using BlockFactory.Entity_;
using BlockFactory.Game;
using BlockFactory.Util.Math_;

namespace BlockFactory.Server.Game
{
    public class ServerSideHandler : ISideHandler
    {
        private readonly BlockFactoryServer _server;

        public ServerSideHandler(BlockFactoryServer server)
        {
            _server = server;
        }

        public void SetEntityPos(Entity entity, EntityPos pos)
        {
            entity.Pos = pos;
        }
    }
}

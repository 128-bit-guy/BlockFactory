using BlockFactory.Base;
using BlockFactory.Content.Entity_;
using BlockFactory.Content.Entity_.Player;
using BlockFactory.Content.Gui.Menu_;
using BlockFactory.Network.Packet_;

namespace BlockFactory.Content.Gui;

[ExclusiveTo(Side.Server)]
public class ServerMenuManager : MenuManager
{
    private readonly ServerPlayerEntity _player;

    public ServerMenuManager(ServerPlayerEntity player): base(true)
    {
        _player = player;
    }

    public override void Push(Menu menu)
    {
        base.Push(menu);
        _player.World!.LogicProcessor.NetworkHandler.SendPacket(_player, new OpenMenuPacket((SynchronizedMenu)menu));
    }

    public override void Pop()
    {
        base.Pop();
        _player.World!.LogicProcessor.NetworkHandler.SendPacket(_player, new CloseMenuPacket());
    }
}
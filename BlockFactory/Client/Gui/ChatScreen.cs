using OpenTK.Mathematics;
using BlockFactory.Network;
using BlockFactory.Side_;

namespace BlockFactory.Client.Gui;

[ExclusiveTo(Side.Client)]
public class ChatScreen : Screen
{
    public TextInputWidget Input;
    public ChatScreen(BlockFactoryClient client) : base(client)
    {
    }

    public override void InitWidgets(Vector2i size)
    {
        base.InitWidgets(size);
        Widgets.Add(Input = new TextInputWidget(this, new Box2(0, size.Y - 120, size.X, size.Y)));
        Input.OnEnterPressed += Send;
    }

    private void Send()
    {
        Client.ServerConnection!.SendPacket(new MessagePacket(Input.Text));
        Client.PopScreen();
    }
}
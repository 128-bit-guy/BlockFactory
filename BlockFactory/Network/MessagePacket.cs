using BlockFactory.Entity_.Player;
using BlockFactory.Game;
using BlockFactory.Server;

namespace BlockFactory.Network;

public class MessagePacket : IPacket
{
    public readonly string Msg;

    public MessagePacket(BinaryReader reader)
    {
        Msg = reader.ReadString();
    }

    public MessagePacket(string msg)
    {
        Msg = msg;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Msg);
    }

    public void Process(NetworkConnection connection)
    {
        Console.WriteLine($"[{connection.Socket.RemoteEndPoint}]: {this.Msg}");
        connection.GameInstance!.EnqueueWork(() =>
        {
            if (this.Msg.StartsWith('/'))
                BlockFactoryServer.Instance.HandleCommand((PlayerEntity)connection.SideObject!, this.Msg);
            else
                foreach (var networkConnection in BlockFactoryServer.Instance.Connections)
                    networkConnection.SendPacket(
                        new OtherPlayerMessagePacket(connection.Socket.RemoteEndPoint!.ToString()!, this.Msg));
        });
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend;
    }
}
using BlockFactory.Game;

namespace BlockFactory.Network;

public class OtherPlayerMessagePacket : IPacket
{
    public readonly string Msg;
    public readonly string Player;

    public OtherPlayerMessagePacket(BinaryReader reader)
    {
        Player = reader.ReadString();
        Msg = reader.ReadString();
    }

    public OtherPlayerMessagePacket(string player, string msg)
    {
        Player = player;
        Msg = msg;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Player);
        writer.Write(Msg);
    }

    public void Process(NetworkConnection connection)
    {
        Console.WriteLine("[{0}]: {1}", this.Player, this.Msg);
    }

    public bool SupportsGameKind(GameKind kind)
    {
        return kind == GameKind.MultiplayerFrontend;
    }
}
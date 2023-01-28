namespace BlockFactory.Network;

public class OtherPlayerMessagePacket : IPacket
{
    public readonly string Player;
    public readonly string Msg;
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
}
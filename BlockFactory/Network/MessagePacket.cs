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
        this.Msg = msg;
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Msg);
    }
}
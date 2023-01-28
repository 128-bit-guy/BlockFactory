namespace BlockFactory.Network
{
    public class PlayerJoinWorldPacket : IPacket
    {
        public readonly long Id;
        public PlayerJoinWorldPacket(BinaryReader reader)
        {
            Id = reader.ReadInt64();
        }
        public PlayerJoinWorldPacket(long id)
        {
            Id = id;
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(Id);
        }
    }
}

using BlockFactory.Registry_;

namespace BlockFactory.Network
{
    public class RegistrySyncPacket : IPacket
    {
        public readonly (RegistryName, RegistryName[])[] Data;
        public RegistrySyncPacket((RegistryName, RegistryName[])[] data) { 
            Data = data;
        }
        public RegistrySyncPacket(BinaryReader reader)
        {
            Data = new (RegistryName, RegistryName[])[reader.Read7BitEncodedInt()];
            for (int i = 0; i < Data.Length; ++i)
            {
                RegistryName name = new(reader);
                RegistryName[] order = new RegistryName[reader.Read7BitEncodedInt()];
                for (int j = 0; j < order.Length; ++j)
                {
                    order[j] = new RegistryName(reader);
                }
                Data[i] = (name, order);
            }
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(Data.Length);
            for (int i = 0; i < Data.Length; ++i)
            {
                var (name, order) = Data[i];
                name.Write(writer);
                writer.Write7BitEncodedInt(order.Length);
                for (int j = 0; j < order.Length; ++j)
                {
                    order[j].Write(writer);
                }
            }
        }
    }
}

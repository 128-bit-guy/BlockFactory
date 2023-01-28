using BlockFactory.Gui;
using BlockFactory.Init;
using BlockFactory.Util;

namespace BlockFactory.Network;

public class InGameMenuOpenPacket : IPacket
{
    public readonly int Id;
    public readonly byte[] Data;

    public InGameMenuOpenPacket(BinaryReader reader)
    {
        Id = reader.Read7BitEncodedInt();
        int length = reader.Read7BitEncodedInt();
        Data = reader.ReadBytes(length);
    }

    public InGameMenuOpenPacket(InGameMenu? menu)
    {
        if (menu == null)
        {
            Id = -1;
            Data = Array.Empty<byte>();
        }
        else
        {
            Id = menu.Type.Id;
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            menu.Write(writer);
            writer.Flush();
            Data = stream.ToArray();
        }
    }
    public void Write(BinaryWriter writer)
    {
        writer.Write7BitEncodedInt(Id);
        writer.Write7BitEncodedInt(Data.Length);
        writer.Write(Data);
    }

    public InGameMenu? CreateMenu()
    {
        if (Id == -1)
        {
            return null;
        }
        else
        {
            using MemoryStream stream = new MemoryStream(Data);
            using BinaryReader reader = new BinaryReader(stream);
            return InGameMenuTypes.Registry[Id].Loader(reader);
        }
    }
}
using BlockFactory.Gui;
using BlockFactory.Init;

namespace BlockFactory.Network;

public class InGameMenuOpenPacket : IPacket
{
    public readonly byte[] Data;
    public readonly int Id;

    public InGameMenuOpenPacket(BinaryReader reader)
    {
        Id = reader.Read7BitEncodedInt();
        var length = reader.Read7BitEncodedInt();
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
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
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

        using var stream = new MemoryStream(Data);
        using var reader = new BinaryReader(stream);
        return InGameMenuTypes.Registry[Id].Loader(reader);
    }
}
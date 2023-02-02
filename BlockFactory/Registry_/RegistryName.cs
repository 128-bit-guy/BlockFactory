namespace BlockFactory.Registry_;

public struct RegistryName : IEquatable<RegistryName>
{
    public string Namespace;
    public string Name;

    public RegistryName(string @namespace, string name)
    {
        Namespace = @namespace;
        Name = name;
    }

    public RegistryName(string name)
    {
        var parts = name.Split(':');
        if (parts.Length > 2) throw new ArgumentException("Name should not contain more than 2 components");

        if (parts.Length == 2)
        {
            Namespace = parts[0];
            Name = parts[1];
        }
        else
        {
            Namespace = "VoxelBuilder";
            Name = parts[0];
        }
    }

    public RegistryName(BinaryReader reader)
    {
        Namespace = reader.ReadString();
        Name = reader.ReadString();
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write(Namespace);
        writer.Write(Name);
    }

    public override string ToString()
    {
        return Namespace + ':' + Name;
    }

    public override bool Equals(object? obj)
    {
        return obj is RegistryName name && Equals(name);
    }

    public bool Equals(RegistryName other)
    {
        return Namespace == other.Namespace &&
               Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Namespace, Name);
    }

    public static bool operator ==(RegistryName left, RegistryName right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RegistryName left, RegistryName right)
    {
        return !(left == right);
    }
}
namespace BlockFactory.Resource;

public abstract class ResourceLoader
{
    public abstract Stream? GetResourceStream(string s);

    public string? GetResourceText(string s)
    {
        using var stream = GetResourceStream(s);
        if (stream == null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
using BlockFactory.Side_;

namespace BlockFactory.Client;

[ExclusiveTo(Side.Client)]
public static class ResourceLoader
{
    public static Stream? GetResourceStream(string s)
    {
        return typeof(ResourceLoader).Assembly.GetManifestResourceStream(s);
    }

    public static StreamReader GetResourceStreamReader(string s)
    {
        return new StreamReader(GetResourceStream(s) ?? throw new InvalidOperationException());
    }

    public static string GetResourceContent(string s)
    {
        using (StreamReader sr = GetResourceStreamReader(s))
        {
            return sr.ReadToEnd();
        }
    }
}
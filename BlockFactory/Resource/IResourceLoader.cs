using BlockFactory.Side_;

namespace BlockFactory.Resource;

[ExclusiveTo(Side.Client)]
public interface IResourceLoader
{
    Stream? GetResourceStream(string location);

    string GetResourceContent(string location)
    {
        using var stream = GetResourceStream(location);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
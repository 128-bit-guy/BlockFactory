using BlockFactory.Serialization;

namespace BlockFactory.Registry_;

public class RegistryMapping : ITagSerializable
{
    public readonly Dictionary<string, Dictionary<string, int>> Mappings = new();
    public DictionaryTag SerializeToTag()
    {
        var result = new DictionaryTag();
        foreach (var (id, mapping) in Mappings)
        {
            var tag = new DictionaryTag();
            foreach (var (entryStrId, entryNumId) in mapping)
            {
                tag.SetValue(entryStrId, entryNumId);
            }
            result.Set(id, tag);
        }

        return result;
    }

    public void DeserializeFromTag(DictionaryTag tag)
    {
        Mappings.Clear();
        foreach (var id in tag.Keys)
        {
            var mappingDict = new Dictionary<string, int>();
            var mapping = tag.Get<DictionaryTag>(id);
            foreach (var entryStrId in mapping.Keys)
            {
                var entryNumId = mapping.GetValue<int>(entryStrId);
                mappingDict.Add(entryStrId, entryNumId);
            }
            Mappings.Add(id, mappingDict);
        }
    }
}
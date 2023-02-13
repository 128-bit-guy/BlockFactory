using BlockFactory.Serialization.Tag;
using OpenTK.Mathematics;

namespace BlockFactory.Serialization;

public static class VectorSerialization
{
    public static ListTag SerializeToTag(this Vector3i vec)
    {
        var res = new ListTag(3, TagType.Int32);
        for (var i = 0; i < 3; ++i)
        {
            res.SetValue(i, vec[i]);
        }

        return res;
    }

    public static Vector3i DeserializeV3I(ListTag tag)
    {
        var res = new Vector3i();
        for (int i = 0; i < 3; ++i)
        {
            res[i] = tag.GetValue<int>(i);
        }

        return res;
    }
}
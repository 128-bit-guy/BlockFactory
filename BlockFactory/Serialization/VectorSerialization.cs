using BlockFactory.Serialization.Tag;
using OpenTK.Mathematics;

namespace BlockFactory.Serialization;

public static class VectorSerialization
{
    public static ListTag SerializeToTag(this Vector3i vec)
    {
        var res = new ListTag(3, TagType.Int32);
        for (var i = 0; i < 3; ++i) res.SetValue(i, vec[i]);

        return res;
    }

    public static Vector3i DeserializeV3I(ListTag tag)
    {
        var res = new Vector3i();
        for (var i = 0; i < 3; ++i) res[i] = tag.GetValue<int>(i);

        return res;
    }

    public static ListTag SerializeToTag(this Vector3 vec)
    {
        var res = new ListTag(3, TagType.Single);
        for (var i = 0; i < 3; ++i) res.SetValue(i, vec[i]);

        return res;
    }

    public static Vector3 DeserializeV3(ListTag tag)
    {
        var res = new Vector3();
        for (var i = 0; i < 3; ++i) res[i] = tag.GetValue<float>(i);

        return res;
    }

    public static ListTag SerializeToTag(this Vector2 vec)
    {
        var res = new ListTag(2, TagType.Single);
        for (var i = 0; i < 2; ++i) res.SetValue(i, vec[i]);

        return res;
    }

    public static Vector2 DeserializeV2(ListTag tag)
    {
        var res = new Vector2();
        for (var i = 0; i < 2; ++i) res[i] = tag.GetValue<float>(i);

        return res;
    }
}
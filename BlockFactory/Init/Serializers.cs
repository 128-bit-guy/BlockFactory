using BlockFactory.Serialization.Automatic;
using OpenTK.Mathematics;

namespace BlockFactory.Init;

public class Serializers
{
    public static void Init()
    {
        var manager = SerializationManager.Common;
        manager.RegisterSpecialSerializer(typeof(Vector3), new VectorSpecialSerializer());
        manager.RegisterSpecialSerializer(typeof(Vector3i), new VectorSpecialSerializer());
        manager.RegisterSpecialSerializer(typeof(Vector2), new VectorSpecialSerializer());
    }
}
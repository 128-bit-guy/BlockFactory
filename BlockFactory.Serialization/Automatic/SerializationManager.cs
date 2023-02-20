using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Serialization.Automatic.Special;

namespace BlockFactory.Serialization.Automatic;

public class SerializationManager
{
    private readonly SerializerBuilder _builder;
    private readonly ConcurrentQueue<(Type, AutoSerializer)> _serializerAddQueue;
    private readonly Dictionary<Type, AutoSerializer> _serializers;
    private readonly Dictionary<Type, ISpecialSerializer> _specialAttributeSerializers;
    private readonly Dictionary<Type, List<ISpecialSerializer>> _specialSerializers;
    public static readonly SerializationManager Common = new();

    public SerializationManager()
    {
        _serializers = new Dictionary<Type, AutoSerializer>();
        _serializerAddQueue = new ConcurrentQueue<(Type, AutoSerializer)>();
        _builder = new SerializerBuilder(this);
        _specialSerializers = new Dictionary<Type, List<ISpecialSerializer>>();
        _specialAttributeSerializers = new Dictionary<Type, ISpecialSerializer>();
        AddDefaultSpecialSerializers();
    }

    public AutoSerializer GetSerializer(Type t)
    {
        if (_serializers.TryGetValue(t, out var serializer)) return serializer;

        foreach (var (t1, serializer2) in _serializerAddQueue)
            if (t1 == t)
                return serializer2;

        var serializer1 = CreateSerializer(t);
        _serializerAddQueue.Enqueue((t, serializer1));
        return serializer1;
    }

    private List<ISpecialSerializer> GetSerializers(Type t)
    {
        if (_specialSerializers.TryGetValue(t, out var res))
        {
            return res;
        }

        res = new List<ISpecialSerializer>();
        _specialSerializers.Add(t, res);
        return res;
    }

    public void RegisterSpecialSerializer(Type type, ISpecialSerializer specialSerializer)
    {
        GetSerializers(type).Add(specialSerializer);
    }

    public void RegisterSpecialAttributeSerializer(Type type, ISpecialSerializer specialSerializer)
    {
        _specialAttributeSerializers.Add(type, specialSerializer);
    }

    private void AddDefaultSpecialSerializers()
    {
        RegisterSpecialAttributeSerializer(typeof(NotSerializedAttribute), new NotSerializedSerializer());
        RegisterSpecialSerializer(typeof(int), new PrimitiveSerializer());
        RegisterSpecialSerializer(typeof(long), new PrimitiveSerializer());
        RegisterSpecialSerializer(typeof(byte), new PrimitiveSerializer());
        RegisterSpecialSerializer(typeof(string), new PrimitiveSerializer());
        RegisterSpecialSerializer(typeof(float), new PrimitiveSerializer());
        RegisterSpecialSerializer(typeof(bool), new PrimitiveSerializer());
        RegisterSpecialSerializer(typeof(ValueType), new EnumSerializer());
        RegisterSpecialAttributeSerializer(typeof(RangeAttribute), new PrimitiveSerializer());
    }

    private AutoSerializer CreateSerializer(Type t)
    {
        return _builder.Build(t);
    }

    public void AddEnqueuedSerializers()
    {
        foreach (var (t, serializer) in _serializerAddQueue) _serializers[t] = serializer;

        _serializerAddQueue.Clear();
    }

    internal bool GetSpecialExpressions(Type t, MemberInfo fieldOrProperty, SerializationExpressionType type,
        ParameterExpression[] parameters, ParameterExpression[] variables, List<Expression> res)
    {
        foreach (var attribute in fieldOrProperty.GetCustomAttributes())
        {
            if (!_specialAttributeSerializers.TryGetValue(attribute.GetType(), out var specialSerializer)) continue;
            if (specialSerializer.GenerateSpecialExpressions(t, fieldOrProperty, attribute, type, parameters,
                    variables, res))
                return true;
        }

        var fieldOrPropertyType = ReflectionUtils.GetFieldOrPropertyType(fieldOrProperty);
        while (true)
        {
            if (_specialSerializers.TryGetValue(fieldOrPropertyType, out var serializers))
            {
                if (serializers.Any(serializer => serializer.GenerateSpecialExpressions(t, fieldOrProperty, null,
                        type, parameters, variables, res)))
                {
                    return true;
                }
            }

            if (fieldOrPropertyType.BaseType == null || fieldOrPropertyType == typeof(object))
            {
                return false;
            }

            fieldOrPropertyType = fieldOrPropertyType.BaseType;
        }
    }
}
using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Serialization.Automatic.ExpressionTypeGenerator;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic;

internal class SerializerBuilder
{
    private readonly Dictionary<SerializationExpressionType, IExpressionTypeGenerator> _generators;
    private readonly SerializationManager _manager;

    internal SerializerBuilder(SerializationManager manager)
    {
        _manager = manager;
        _generators = new Dictionary<SerializationExpressionType, IExpressionTypeGenerator>();
        _generators[SerializationExpressionType.FromTag] = new FromTagGenerator(this);
        _generators[SerializationExpressionType.ToTag] = new ToTagGenerator(this);
        _generators[SerializationExpressionType.FromBinaryReader] = new FromBinaryReaderGenerator(this);
        _generators[SerializationExpressionType.ToBinaryWriter] = new ToBinaryWriterGenerator(this);
    }

    private T BuildSerializerOfType<T>(Type t, SerializationExpressionType type) where T : Delegate
    {
        var generator = _generators[type];
        var parameterExpressions = generator.GetParameters(t);
        var variableExpressions = generator.GetVariables(t);
        var resUnjoined = new List<Expression>();
        resUnjoined.AddRange(generator.GetBeginning(
            t,
            parameterExpressions,
            variableExpressions
        ));
        foreach (var field in t.GetFields())
        {
            if ((field.Attributes & FieldAttributes.Static) == FieldAttributes.Static) continue;

            if (!_manager.GetSpecialExpressions(
                    t,
                    field,
                    type,
                    parameterExpressions,
                    variableExpressions,
                    resUnjoined
                ))
                resUnjoined.AddRange(generator.GetFieldOrProperty(
                    t,
                    field,
                    parameterExpressions,
                    variableExpressions));
        }

        foreach (var property in t.GetProperties())
            if (!_manager.GetSpecialExpressions(
                    t,
                    property,
                    type,
                    parameterExpressions,
                    variableExpressions,
                    resUnjoined
                ))
                resUnjoined.AddRange(generator.GetFieldOrProperty(
                    t,
                    property,
                    parameterExpressions,
                    variableExpressions));

        resUnjoined.AddRange(generator.GetEnding(t, parameterExpressions, variableExpressions));
        var resJoined =
            Expression.Block(generator.GetReturnType(), variableExpressions, resUnjoined.ToArray());
        var lambdaExpression =
            Expression.Lambda<T>(resJoined, $"{t.Name} serializer of type {type}", parameterExpressions);
        return lambdaExpression.Compile();
    }

    internal AutoSerializer Build(Type t)
    {
        var toTag = BuildSerializerOfType<Func<object, DictionaryTag>>(t, SerializationExpressionType.ToTag);
        var fromTag = BuildSerializerOfType<Action<object, DictionaryTag>>(t, SerializationExpressionType.FromTag);
        var toBinaryWriter =
            BuildSerializerOfType<Action<object, BinaryWriter>>(t, SerializationExpressionType.ToBinaryWriter);
        var fromBinaryReader =
            BuildSerializerOfType<Action<object, BinaryReader>>(t, SerializationExpressionType.FromBinaryReader);
        return new AutoSerializer(toTag, fromTag, toBinaryWriter, fromBinaryReader);
    }
}
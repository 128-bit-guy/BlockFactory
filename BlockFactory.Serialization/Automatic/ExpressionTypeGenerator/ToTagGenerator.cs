using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic.ExpressionTypeGenerator;

internal class ToTagGenerator : IExpressionTypeGenerator
{
    private readonly SerializerBuilder _builder;

    internal ToTagGenerator(SerializerBuilder builder)
    {
        _builder = builder;
    }

    public ParameterExpression[] GetParameters(Type t)
    {
        return new[] { Expression.Parameter(typeof(object), "obj") };
    }

    public ParameterExpression[] GetVariables(Type t)
    {
        return new[]
        {
            Expression.Variable(t, "obj2"),
            Expression.Variable(typeof(DictionaryTag), "res")
        };
    }

    public IEnumerable<Expression> GetBeginning(Type t, ParameterExpression[] parameters,
        ParameterExpression[] variables)
    {
        Expression res = Expression.Assign(variables[1], Expression.New(typeof(DictionaryTag)));
        Expression obj2 =
            Expression.Assign(variables[0], Expression.Convert(parameters[0], t));
        return new[] { res, obj2 };
    }

    public IEnumerable<Expression> GetFieldOrProperty(Type t, MemberInfo fieldOrProperty,
        ParameterExpression[] parameters,
        ParameterExpression[] variables)
    {
        var fieldType = ReflectionUtils.GetFieldOrPropertyType(fieldOrProperty);
        if (fieldType.IsAssignableTo(typeof(ITagSerializable)))
        {
            var serialize = typeof(ITagSerializable).GetMethod(nameof(ITagSerializable.SerializeToTag))!;
            Expression tag = Expression.Call(
                Expression.MakeMemberAccess(variables[0], fieldOrProperty),
                serialize
            );
            var set = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.Set))!;
            Expression fieldName = Expression.Constant(fieldOrProperty.Name);
            Expression setTag = Expression.Call(variables[1], set, fieldName, tag);
            return new[] { setTag };
        }

        throw new ArgumentException($"Serialized field should implement {nameof(ITagSerializable)}");
    }

    public IEnumerable<Expression> GetEnding(Type t, ParameterExpression[] parameters, ParameterExpression[] variables)
    {
        return new Expression[]
            { variables[1] };
    }

    public Type GetReturnType()
    {
        return typeof(DictionaryTag);
    }
}
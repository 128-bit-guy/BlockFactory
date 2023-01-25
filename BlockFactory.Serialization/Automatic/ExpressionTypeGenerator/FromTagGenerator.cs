using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Serialization.Serializable;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic.ExpressionTypeGenerator;

internal class FromTagGenerator : IExpressionTypeGenerator
{
    private readonly SerializerBuilder _builder;

    internal FromTagGenerator(SerializerBuilder builder)
    {
        _builder = builder;
    }

    public ParameterExpression[] GetParameters(Type t)
    {
        return new[]
        {
            Expression.Parameter(typeof(object), "obj"),
            Expression.Parameter(typeof(DictionaryTag), "tag")
        };
        // throw new NotImplementedException();
    }

    public ParameterExpression[] GetVariables(Type t)
    {
        return new[] { Expression.Parameter(t, "obj2") };
        // throw new NotImplementedException();
    }

    public IEnumerable<Expression> GetBeginning(Type t, ParameterExpression[] parameters,
        ParameterExpression[] variables)
    {
        Expression obj2 =
            Expression.Assign(variables[0], Expression.Convert(parameters[0], t));
        return new[] { obj2 };
        // throw new NotImplementedException();
    }

    public IEnumerable<Expression> GetFieldOrProperty(Type t, MemberInfo fieldOrProperty,
        ParameterExpression[] parameters,
        ParameterExpression[] variables)
    {
        var fieldType = ReflectionUtils.GetFieldOrPropertyType(fieldOrProperty);
        if (fieldType.IsAssignableTo(typeof(ITagSerializable)))
        {
            var deserialize = typeof(ITagSerializable).GetMethod(nameof(ITagSerializable.DeserializeFromTag))!;
            var get = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.Get))!;
            var getBuild = get.MakeGenericMethod(typeof(DictionaryTag));
            Expression fieldName = Expression.Constant(fieldOrProperty.Name);
            Expression tag = Expression.Call(
                parameters[1],
                getBuild,
                fieldName
            );
            Expression deserializeExpr = Expression.Call(
                Expression.MakeMemberAccess(variables[0], fieldOrProperty),
                deserialize,
                tag
            );
            return new[] { deserializeExpr };
        }

        throw new ArgumentException($"Serialized field should implement {nameof(ITagSerializable)}");
        // throw new NotImplementedException();
    }

    public IEnumerable<Expression> GetEnding(Type t, ParameterExpression[] parameters, ParameterExpression[] variables)
    {
        return Array.Empty<Expression>();
        // throw new NotImplementedException();
    }

    public Type GetReturnType()
    {
        return typeof(void);
    }
}
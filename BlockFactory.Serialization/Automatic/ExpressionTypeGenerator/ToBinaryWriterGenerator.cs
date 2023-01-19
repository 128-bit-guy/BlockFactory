using System.Linq.Expressions;
using System.Reflection;

namespace BlockFactory.Serialization.Automatic.ExpressionTypeGenerator;

internal class ToBinaryWriterGenerator : IExpressionTypeGenerator
{
    private readonly SerializerBuilder _builder;

    internal ToBinaryWriterGenerator(SerializerBuilder builder)
    {
        _builder = builder;
    }

    public ParameterExpression[] GetParameters(Type t)
    {
        return new[]
        {
            Expression.Parameter(typeof(object), "obj"),
            Expression.Parameter(typeof(BinaryWriter), "writer")
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
        return Array.Empty<Expression>();
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
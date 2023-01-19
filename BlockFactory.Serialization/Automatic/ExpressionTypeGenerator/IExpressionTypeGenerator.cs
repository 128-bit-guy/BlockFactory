using System.Linq.Expressions;
using System.Reflection;

namespace BlockFactory.Serialization.Automatic.ExpressionTypeGenerator;

internal interface IExpressionTypeGenerator
{
    ParameterExpression[] GetParameters(Type t);
    ParameterExpression[] GetVariables(Type t);

    IEnumerable<Expression> GetBeginning(Type t, ParameterExpression[] parameters,
        ParameterExpression[] variables);

    IEnumerable<Expression> GetFieldOrProperty(Type t, MemberInfo fieldOrProperty,
        ParameterExpression[] parameters,
        ParameterExpression[] variables);

    IEnumerable<Expression> GetEnding(Type t, ParameterExpression[] parameters, ParameterExpression[] variables);
    Type GetReturnType();
}
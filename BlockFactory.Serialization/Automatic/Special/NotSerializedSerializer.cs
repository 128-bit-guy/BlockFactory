﻿using System.Linq.Expressions;
using System.Reflection;

namespace BlockFactory.Serialization.Automatic.Special;

internal class NotSerializedSerializer : ISpecialSerializer
{
    public bool GenerateSpecialExpressions(Type t, MemberInfo fieldOrProperty, object? attribute,
        SerializationExpressionType type,
        ParameterExpression[] parameters, ParameterExpression[] variables, List<Expression> res)
    {
        return true;
    }
}
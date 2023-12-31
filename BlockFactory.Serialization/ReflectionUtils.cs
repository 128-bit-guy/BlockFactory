﻿using System.Linq.Expressions;
using System.Reflection;

namespace BlockFactory.Serialization;

public static class ReflectionUtils
{
    /// <summary>
    ///     Creates delegate from constructor
    /// </summary>
    /// <param name="info"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateDelegate<T>(this ConstructorInfo info) where T : Delegate
    {
        var parameterExpressions =
            info.GetParameters().Select(x => Expression.Parameter(x.ParameterType)).ToArray();
        // ReSharper disable once CoVariantArrayConversion
        var expression = Expression.Lambda<T>(Expression.New(info, parameterExpressions), parameterExpressions);
        return expression.Compile();
    }
}
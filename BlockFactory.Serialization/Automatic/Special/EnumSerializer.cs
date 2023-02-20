using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic.Special;

public class EnumSerializer : ISpecialSerializer
{
    public bool GenerateSpecialExpressions(Type t, MemberInfo fieldOrProperty, object? attribute, SerializationExpressionType type,
        ParameterExpression[] parameters, ParameterExpression[] variables, List<Expression> res)
    {
        var targetType = ReflectionUtils.GetFieldOrPropertyType(fieldOrProperty);
        if (!targetType.IsEnum)
        {
            return false;
        }
        Expression targetExpr = Expression.MakeMemberAccess(variables[0], fieldOrProperty);
        Expression targetName = Expression.Constant(fieldOrProperty.Name);
        switch (type)
        {
            case SerializationExpressionType.ToTag:
            {
                var tagExpr = variables[1];
                var setValue = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.SetValue))!;
                var setValueBuilt = setValue.MakeGenericMethod(typeof(int));
                var castExpr = Expression.Convert(targetExpr, typeof(int));
                var setValueExpr = Expression.Call(
                    tagExpr,
                    setValueBuilt,
                    targetName,
                    castExpr
                );
                res.Add(setValueExpr);
                return true;
            }
            case SerializationExpressionType.FromTag:
            {
                var tagExpr = parameters[1];
                var getValue = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.GetValue))!;
                var getValueBuilt = getValue.MakeGenericMethod(typeof(int));
                var getValueExpr = Expression.Call(
                    tagExpr,
                    getValueBuilt,
                    targetName
                );
                var castExpr = Expression.Convert(getValueExpr, targetType);
                var assign = Expression.Assign(targetExpr, castExpr);
                res.Add(assign);
                return true;
            }
            case SerializationExpressionType.FromBinaryReader:
            case SerializationExpressionType.ToBinaryWriter:
                return true;
        }

        return false;
    }
}
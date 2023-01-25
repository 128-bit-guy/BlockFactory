using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic.Special;

internal class PrimitiveSerializer : ISpecialSerializer
{
    public bool GenerateSpecialExpressions(Type t, MemberInfo fieldOrProperty, object? attribute,
        SerializationExpressionType type,
        ParameterExpression[] parameters, ParameterExpression[] variables, List<Expression> res)
    {
        var targetType = ReflectionUtils.GetFieldOrPropertyType(fieldOrProperty);
        Expression targetExpr = Expression.MakeMemberAccess(variables[0], fieldOrProperty);
        Expression targetName = Expression.Constant(fieldOrProperty.Name);
        switch (type)
        {
            case SerializationExpressionType.ToTag:
            {
                var tagExpr = variables[1];
                var setValue = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.SetValue))!;
                var setValueBuilt = setValue.MakeGenericMethod(targetType);
                var setValueExpr = Expression.Call(
                    tagExpr,
                    setValueBuilt,
                    targetName,
                    targetExpr
                );
                res.Add(setValueExpr);
                return true;
            }
            case SerializationExpressionType.FromTag:
            {
                var tagExpr = parameters[1];
                var getValue = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.GetValue))!;
                var getValueBuilt = getValue.MakeGenericMethod(targetType);
                var getValueExpr = Expression.Call(
                    tagExpr,
                    getValueBuilt,
                    targetName
                );
                var clampExpr = GenerateClampIfNecessary(getValueExpr, targetType, attribute);
                var assign = Expression.Assign(targetExpr, clampExpr);
                res.Add(assign);
                return true;
            }
            case SerializationExpressionType.FromBinaryReader:
            case SerializationExpressionType.ToBinaryWriter:
                return true;
        }

        return false;
    }

    private static Expression GenerateClampIfNecessary(Expression baseExpression, Type fieldType, object? attribute)
    {
        if (attribute == null) return baseExpression;

        var rangeAttribute = (RangeAttribute)attribute;
        Expression min =
            Expression.Constant(
                Convert.ChangeType(rangeAttribute.FloatingPoint ? rangeAttribute.MinFP : (object)rangeAttribute.Min,
                    fieldType), fieldType);
        Expression max =
            Expression.Constant(
                Convert.ChangeType(rangeAttribute.FloatingPoint ? rangeAttribute.MaxFP : (object)rangeAttribute.Max,
                    fieldType), fieldType);
        var clamp = typeof(Math).GetMethod(nameof(Math.Clamp), new[] { fieldType, fieldType, fieldType })!;
        Expression clamped = Expression.Call(null, clamp, baseExpression, min, max);
        return clamped;
    }
}
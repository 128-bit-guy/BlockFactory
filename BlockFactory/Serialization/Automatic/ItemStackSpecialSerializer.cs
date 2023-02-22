using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Item_;
using BlockFactory.Serialization.Automatic.Special;
using BlockFactory.Serialization.Tag;

namespace BlockFactory.Serialization.Automatic;

public class ItemStackSpecialSerializer : ISpecialSerializer
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
                var set = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.Set))!;
                var serializeExpr = Expression.Call(targetExpr,
                    typeof(ItemStack).GetMethod(nameof(ItemStack.SerializeToTag))!);
                var setExpr = Expression.Call(
                    tagExpr,
                    set,
                    targetName,
                    serializeExpr
                );
                res.Add(setExpr);
                return true;
            }
            case SerializationExpressionType.FromTag:
            {
                var tagExpr = parameters[1];
                var get = typeof(DictionaryTag).GetMethod(nameof(DictionaryTag.Get))!;
                var getBuilt = get.MakeGenericMethod(typeof(DictionaryTag));
                var getExpr = Expression.Call(
                    tagExpr,
                    getBuilt,
                    targetName
                );
                var deserializeExpr =
                    Expression.New(typeof(ItemStack).GetConstructor(new Type[] { typeof(DictionaryTag) })!, getExpr);
                var assign = Expression.Assign(targetExpr, deserializeExpr);
                res.Add(assign);
                return true;
            }
            case SerializationExpressionType.ToBinaryWriter:
            case SerializationExpressionType.FromBinaryReader:
                return true;
        }

        return false;
    }
}
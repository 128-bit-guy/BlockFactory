using System.Linq.Expressions;
using System.Reflection;
using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
public static class VertexTransformationInfo<T> where T : unmanaged
{
    public delegate void VertexTransformer(ref T vertex, MeshBuilder<T> builder);

    public static readonly VertexTransformer Transformer = CreateTransformer();

    private static VertexTransformer CreateTransformer()
    {
        var vertex = Expression.Parameter(typeof(T).MakeByRefType(), "vertex");
        var builder = Expression.Parameter(typeof(MeshBuilder<T>), "builder");
        var transformers = typeof(VertexFieldTransformers<T>);
        var expressions = (from field in typeof(T).GetFields()
            let transformType =
                (TransformationTypeAttribute?)field.GetCustomAttributes(true)
                    .FirstOrDefault(a => a is TransformationTypeAttribute)
            where transformType != null
            let fieldExpr = Expression.Field(vertex, field)
            let m = transformers.GetMethod($"Transform{transformType.TransformType.ToString()}",
                new[] { field.FieldType, typeof(MeshBuilder<T>) })!
            select Expression.Assign(fieldExpr, Expression.Call(m, fieldExpr, builder))).Cast<Expression>().ToList();
        var block = Expression.Block(expressions);
        var lambda = Expression.Lambda<VertexTransformer>(block, vertex, builder);
        return lambda.Compile();
    }
}
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static class VertexInfo<T> where T : unmanaged
{
    private static uint _size;
    private static List<VertexAttributeInfo> _vertexAttributeInfos;
    private static List<(FieldInfo field, TransformType type)> _transformTypes;

    static VertexInfo()
    {
        _size = (uint)Unsafe.SizeOf<T>();
        _vertexAttributeInfos = new List<VertexAttributeInfo>();
        _transformTypes = new List<(FieldInfo field, TransformType type)>();
        var attribTypes = new Dictionary<Type, VertexAttribType>
        {
            [typeof(float)] = VertexAttribType.Float,
            [typeof(double)] = VertexAttribType.Double,
            [typeof(int)] = VertexAttribType.Int
        };
        foreach (var field in typeof(T).GetFields())
        {
            var layoutLocation = (LayoutLocationAttribute?)field
                .GetCustomAttributes(true).FirstOrDefault(a => a is LayoutLocationAttribute);
            if (layoutLocation != null)
            {
                var offset = (uint)Marshal.OffsetOf<T>(field.Name);
                var t = field.FieldType;
                if (t.IsGenericType)
                {
                    var generic = t.GetGenericTypeDefinition();
                    var type = attribTypes[t.GetGenericArguments()[0]];
                    if (generic == typeof(Vector3D<>))
                    {
                        _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 3, type));
                    }
                    else if (generic == typeof(Vector2D<>))
                    {
                        _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 2, type));
                    }
                    else if (generic == typeof(Vector4D<>))
                    {
                        _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 4, type));
                    }
                    else
                    {
                        throw new ArgumentException(
                            $"{typeof(T)} has field {field.Name} of type {t.FullName} which is not supported");
                    }
                }
                else if (t == typeof(Vector3))
                {
                    _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 3,
                        VertexAttribType.Float));
                }
                else if (attribTypes.TryGetValue(t, out var value))
                {
                    _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 1, value));
                }
                else
                {
                    throw new ArgumentException(
                        $"{typeof(T)} has field {field.Name} of type {t.FullName} which is not supported");
                }
            }

            var transformType = (TransformationTypeAttribute?)field
                .GetCustomAttributes(true).FirstOrDefault(a => a is TransformationTypeAttribute);
            if (transformType != null)
            {
                _transformTypes.Add((field, transformType.TransformType));
            }
        }
    }

    public static void AttachVbo(uint vao, uint vbo, uint binding)
    {
        BfRendering.Gl.VertexArrayVertexBuffer(vao, binding, vbo, 0, _size);
        BfRendering.Gl.EnableVertexArrayAttrib(vao, binding);
        foreach (var info in _vertexAttributeInfos)
        {
            BfRendering.Gl.VertexArrayAttribFormat(vao, info.LayoutLocation, info.Count, info.Type,
                false, info.Offset);
            BfRendering.Gl.VertexArrayAttribBinding(vao, info.LayoutLocation, binding);
        }
    }
}
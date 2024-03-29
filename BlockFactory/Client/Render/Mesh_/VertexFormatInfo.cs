﻿using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using BlockFactory.Base;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace BlockFactory.Client.Render.Mesh_;

[ExclusiveTo(Side.Client)]
[SuppressMessage("ReSharper", "StaticMemberInGenericType")]
public static class VertexFormatInfo<T> where T : unmanaged
{
    private static readonly uint _size;
    private static readonly List<VertexAttributeInfo> _vertexAttributeInfos;

    static VertexFormatInfo()
    {
        _size = (uint)Unsafe.SizeOf<T>();
        _vertexAttributeInfos = new List<VertexAttributeInfo>();
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
                        _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 3, type));
                    else if (generic == typeof(Vector2D<>))
                        _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 2, type));
                    else if (generic == typeof(Vector4D<>))
                        _vertexAttributeInfos.Add(new VertexAttributeInfo(layoutLocation.Location, offset, 4, type));
                    else
                        throw new ArgumentException(
                            $"{typeof(T)} has field {field.Name} of type {t.FullName} which is not supported");
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
        }
    }

    public static void AttachVbo(uint vao, uint vbo, uint binding)
    {
        BfRendering.Gl.VertexArrayVertexBuffer(vao, binding, vbo, 0, _size);
        foreach (var info in _vertexAttributeInfos)
        {
            BfRendering.Gl.EnableVertexArrayAttrib(vao, info.LayoutLocation);
            if (info.Type == VertexAttribType.Int)
            {
                BfRendering.Gl.VertexArrayAttribIFormat(vao, info.LayoutLocation, info.Count,
                    VertexAttribIType.Int, info.Offset);
            }
            else
            {
                BfRendering.Gl.VertexArrayAttribFormat(vao, info.LayoutLocation, info.Count, info.Type,
                    false, info.Offset);
            }

            BfRendering.Gl.VertexArrayAttribBinding(vao, info.LayoutLocation, binding);
        }
    }
}
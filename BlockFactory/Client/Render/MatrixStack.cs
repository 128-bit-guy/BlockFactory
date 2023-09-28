﻿using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.Client.Render;

[ExclusiveTo(Side.Client)]
public class MatrixStack
{
    private readonly Stack<Matrix4X4<float>> _stack = new();

    public void Push()
    {
        _stack.Push(_stack.TryPeek(out var mat) ? mat : Matrix4X4<float>.Identity);
    }

    public void Pop()
    {
        _stack.Pop();
    }

    public void Multiply(Matrix4X4<float> mat)
    {
        _stack.Push( mat * _stack.Pop());
    }

    public void Translate(Vector3D<float> delta)
    {
        Multiply(Matrix4X4.CreateTranslation(delta));
    }

    public void Translate(float x, float y, float z)
    {
        Translate(new Vector3D<float>(x, y, z));
    }

    public void Scale(Vector3D<float> scale)
    {
        Multiply(Matrix4X4.CreateScale(scale));
    }

    public void Scale(float x, float y, float z)
    {
        Scale(new Vector3D<float>(x, y, z));
    }

    public void Scale(float s)
    {
        Scale(s, s, s);
    }

    public static implicit operator Matrix4X4<float>(MatrixStack stack)
    {
        return stack._stack.Peek();
    }
}
using OpenTK.Mathematics;

namespace BlockFactory.Client.Render;

public class MatrixStack
{
    private readonly Stack<Matrix4> _matrices;

    public MatrixStack()
    {
        _matrices = new Stack<Matrix4>();
    }

    public void Push()
    {
        _matrices.Push(_matrices.TryPeek(out Matrix4 prev) ? prev : Matrix4.Identity);
    }

    public void Pop()
    {
        _matrices.Pop();
    }

    public void Multiply(Matrix4 mat)
    {
        Matrix4 m = _matrices.Pop();
        m = mat * m;
        _matrices.Push(m);
    }

    public void Translate(Vector3 translation)
    {
        Multiply(Matrix4.CreateTranslation(translation));
    }

    public void RotateX(float angle)
    {
        Multiply(Matrix4.CreateRotationX(angle));
    }

    public void RotateY(float angle)
    {
        Multiply(Matrix4.CreateRotationY(angle));
    }

    public void RotateZ(float angle)
    {
        Multiply(Matrix4.CreateRotationZ(angle));
    }

    public void Scale(Vector3 s)
    {
        Multiply(Matrix4.CreateScale(s));
    }

    public void Scale(float s)
    {
        Multiply(Matrix4.CreateScale(s));
    }

    public void Reset()
    {
        _matrices.Clear();
    }

    public static implicit operator Matrix4(MatrixStack stack)
    {
        return stack._matrices.Peek();
    }
}
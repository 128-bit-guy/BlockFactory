using BlockFactory.Utils;
using Silk.NET.Maths;

namespace BlockFactory.Physics;

public class BoxConsumer
{
    public delegate void BoxConsumerFunc(Box3D<double> box);
    public List<Box3D<double>> Boxes;
    public Vector3D<double> Offset;
    public readonly BoxConsumerFunc Func;

    public BoxConsumer()
    {
        Boxes = new List<Box3D<double>>();
        Func = AddBox;
    }

    public void AddBox(Box3D<double> box)
    {
        Boxes.Add(box.Add(Offset));
    }

    public void Clear()
    {
        Boxes.Clear();
    }
}
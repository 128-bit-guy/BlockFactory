using Silk.NET.Maths;

namespace BlockFactory.World_.Gen.Mineshaft;

public interface IMineshaftElement
{
    public Vector3D<int> GetCenter();
    public void AddNeighbours(Random rng, Queue<IMineshaftElement> elements);
    public void Generate(Chunk c, Random rng);
}
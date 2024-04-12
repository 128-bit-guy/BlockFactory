using BlockFactory.Content.Block_;
using BlockFactory.CubeMath;
using BlockFactory.World_.Interfaces;
using Silk.NET.Maths;

namespace BlockFactory.World_.Gen;

public class OreGenerator
{
    [ThreadStatic] private static List<Vector3D<int>>? _posQueue;
    private readonly short _block;
    private readonly int _maxRadius;
    private readonly int _maxSize;
    private readonly int _minSize;
    private readonly short _replacedBlock;
    private readonly float _veinChance;

    public OreGenerator(short block, short replacedBlock, int minSize, int maxSize, int maxRadius, float veinChance)
    {
        _block = block;
        _replacedBlock = replacedBlock;
        _minSize = minSize;
        _maxSize = maxSize;
        _maxRadius = maxRadius;
        _veinChance = veinChance;
    }

    public OreGenerator(Block block, Block replacedBlock, int minSize, int maxSize, int maxRadius, float veinChance) :
        this((short)block.Id, (short)replacedBlock.Id, minSize, maxSize, maxRadius, veinChance)
    {
    }

    public void Generate(IBlockStorage world, Vector3D<int> pos, Random rng)
    {
        _posQueue ??= new List<Vector3D<int>>();
        if (rng.NextDouble() >= _veinChance) return;

        _posQueue.Add(pos);

        var currentSize = rng.Next(_minSize, _maxSize);

        while (_posQueue.Count > 0 && currentSize > 0)
        {
            var i = rng.Next(_posQueue.Count);
            var cPos = _posQueue[i];
            _posQueue[i] = _posQueue[^1];
            _posQueue.RemoveAt(_posQueue.Count - 1);
            if (world.GetBlock(cPos) != _replacedBlock) continue;
            world.SetBlock(cPos, _block);
            --currentSize;
            foreach (var face in CubeFaceUtils.Values())
            {
                var nPos = cPos + face.GetDelta();
                if ((nPos - pos).LengthSquared <= _maxRadius * _maxRadius) _posQueue.Add(nPos);
            }
        }

        _posQueue.Clear();
    }
}
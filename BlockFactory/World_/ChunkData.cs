using System.Diagnostics;
using System.Runtime.CompilerServices;
using BlockFactory.Base;
using Silk.NET.Maths;

namespace BlockFactory.World_;

public class ChunkData : IBlockStorage
{
    private readonly short[] _blocks = new short[Constants.ChunkSize * Constants.ChunkSize * Constants.ChunkSize];
    
    public short GetBlock(Vector3D<int> pos)
    {
        return _blocks[GetArrIndex(pos)];
    }

    public void SetBlock(Vector3D<int> pos, short block)
    {
        _blocks[GetArrIndex(pos)] = block;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetArrIndex(Vector3D<int> pos)
    {
        return (pos.X & Constants.ChunkMask) | (((pos.Y & Constants.ChunkMask) | ((pos.Z & Constants.ChunkMask)
            << Constants.ChunkSizeLog2)) << Constants.ChunkSizeLog2);
    }
}
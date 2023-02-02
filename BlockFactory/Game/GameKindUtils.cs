using System.Runtime.CompilerServices;

namespace BlockFactory.Game;

public static class GameKindUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DoesProcessLogic(this GameKind kind)
    {
        return ((int)kind & 1) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool DoesRendering(this GameKind kind)
    {
        return ((int)kind & 2) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNetworked(this GameKind kind)
    {
        return kind != GameKind.Singleplayer;
    }
}
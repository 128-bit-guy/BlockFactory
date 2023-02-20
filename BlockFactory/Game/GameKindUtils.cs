using System.Runtime.CompilerServices;
using BlockFactory.Side_;

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

    public static Side GetPhysicalSide(this GameKind kind)
    {
        return kind == GameKind.MultiplayerBackend ? Side.Server : Side.Client;
    }
}
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Backend.Engine.NNUE.Vectorization;

public static class VMethod
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector<T> Clamp<T>(this Vector<T> value, ref Vector<T> min, ref Vector<T> max) where T : struct
    {
        return Vector.Max(min, Vector.Min(max, value));
    }

}
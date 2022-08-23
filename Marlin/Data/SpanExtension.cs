using System.Runtime.CompilerServices;

namespace Marlin.Data;

public static class SpanExtension
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> Till(this ReadOnlySpan<char> input, char c, int occ)
    {
        int lastFound = input.Length;
        for (int i = 0; i < input.Length; i++) {
            if (input[i] == c) {
                occ--;
                lastFound = i;
            }
            if (occ == 0) break;
        }

        return input[..lastFound];
    }

}
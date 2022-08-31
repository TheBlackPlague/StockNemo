using System.Runtime.CompilerServices;

namespace Marlin;

public static class CharSpanExtension
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> Until(this ReadOnlySpan<char> memory, char ch)
    {
        int n = 0;
        int l = memory.Length;
        while (memory[n] != ch && n < l) {
            n++;
        }

        return n < memory.Length ? memory[..n] : memory;
    }

}
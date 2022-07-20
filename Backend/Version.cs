using System.Runtime.CompilerServices;

namespace Backend;

public static class Version
{

    private const string VERSION = "3.0.0.3";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get()
    {
        return VERSION;
    }

}
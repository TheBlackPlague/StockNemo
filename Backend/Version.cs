using System.Runtime.CompilerServices;

namespace Backend;

public static class Version
{

    private const string VERSION = "2.0.0.5";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get()
    {
        return VERSION;
    }

}
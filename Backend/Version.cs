using System.Runtime.CompilerServices;

namespace Backend;

public static class Version
{

    private const string VERSION = "5.7.0.0";

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Get()
    {
        return VERSION;
    }

}
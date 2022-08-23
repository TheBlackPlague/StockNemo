using System.IO.MemoryMappedFiles;

namespace Marlin.Data.Conversion;

public static class ConversionUtil
{

    public static void Convert<From>(string fromPath, string toPath) where From : IEngine, new()
    {
        using Stream input = File.OpenRead(fromPath);
        using Stream output = File.OpenWrite(toPath);
        
        From from = new();
        from.Convert(input, output);
    }

}
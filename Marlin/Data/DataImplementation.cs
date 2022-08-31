using Marlin.Data.Enum;

namespace Marlin.Data;

public abstract class DataImplementation
{

    public readonly Stream DataStream;

    protected DataImplementation(string path, DataOperation op)
    {
        DataStream = (op switch
        {
            DataOperation.Read => File.OpenRead(path),
            DataOperation.Write => File.OpenWrite(path),
            _ => DataStream
        })!;
    }

}
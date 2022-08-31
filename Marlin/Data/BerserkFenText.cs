using Marlin.Data.Enum;
using Marlin.Data.Struct;

namespace Marlin.Data;

public class BerserkFenText : FenText
{

    private const string SEP_0 = " [";
    private const string SEP_1 = "] ";

    public BerserkFenText(string path, DataOperation op) : base(path, op) {}

    protected override PackedDataPoint PackLine(string line)
    {
        throw new NotImplementedException();
    }

}
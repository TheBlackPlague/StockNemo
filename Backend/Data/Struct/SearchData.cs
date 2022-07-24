using System.Runtime.CompilerServices;

namespace Backend.Data.Struct;

public ref struct SearchData
{

    public int PositionalEvaluation;

    unsafe private SearchData* Previous;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref SearchData GetPrevious(int ply)
    {
        SearchData* current = Previous;
        int i = 1;
        while (i < ply) {
            current = current->Previous;
            i++;
        }
        return ref *current;
    }

    public unsafe void SetPrevious(SearchData data) => Previous = &data;

}
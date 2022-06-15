using System.Numerics;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct;

#pragma warning disable CS0660, CS0661
public struct BitBoard
#pragma warning restore CS0660, CS0661
{

    public static readonly BitBoard Default = new(ulong.MinValue);
    public static readonly BitBoard Filled = new(ulong.MaxValue);

    #region Operators
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator +(BitBoard left, BitBoard right)
    {
        left.Internal += right.Internal;
        return left;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator -(BitBoard left, BitBoard right)
    {
        left.Internal -= right.Internal;
        return left;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator *(BitBoard left, BitBoard right)
    {
        left.Internal *= right.Internal;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator /(BitBoard left, BitBoard right)
    {
        left.Internal /= right.Internal;
        return left;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator %(BitBoard to, ulong by)
    {
        to.Internal %= by;
        return to;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator |(BitBoard left, BitBoard right)
    {
        left.Internal |= right.Internal;
        return left;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator &(BitBoard left, BitBoard right)
    {
        left.Internal &= right.Internal;
        return left;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator ~(BitBoard bitBoard)
    {
        bitBoard.Internal = ~bitBoard.Internal;
        return bitBoard;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator >>(BitBoard bitBoard, int by)
    {
        bitBoard.Internal >>= by;
        return bitBoard;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BitBoard operator <<(BitBoard bitBoard, int by)
    {
        bitBoard.Internal <<= by;
        return bitBoard;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BitBoard left, BitBoard right)
    {
        return left.Internal == right.Internal;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BitBoard left, BitBoard right)
    {
        return left.Internal != right.Internal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(BitBoard bitBoard)
    {
        return bitBoard.Internal != 0UL;
    }
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ulong(BitBoard bitBoard)
    {
        return bitBoard.Internal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitBoard(ulong from)
    {
        return new BitBoard(from);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator BitBoard(Square sq)
    {
        BitBoard a = Default;
        a[sq] = true;
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Square(BitBoard bitBoard)
    {
        // Intrinsic: TZCNT
        return (Square)BitOperations.TrailingZeroCount(bitBoard.Internal);
    }

    public static explicit operator Square[](BitBoard bitBoard)
    {
        int c = bitBoard.Count;
        BitBoardIterator iterator = new(bitBoard.Internal, c);
        Square[] a = new Square[c];
        int i = 0;
        while (i != c) {
            a[i++] = iterator.Current;
            iterator.MoveNext();
        }

        return a;
    }
        
    #endregion
        
    // Number of set bits.
    // Intrinsic: POPCNT
    public int Count => BitOperations.PopCount(Internal);

    private ulong Internal;
       
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitBoard(BitBoard from)
    {
        Internal = from.Internal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private BitBoard(ulong from)
    {
        Internal = from;
    }

    #region Indexers

    public bool this[Square sq]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Internal >> (int)sq & 1UL) == 1UL;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (value) Internal |= 1UL << (int)sq;
            else Internal &= ~(1UL << (int)sq);
        }
    }

    public bool this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Internal >> i & 1UL) == 1UL;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            if (value) Internal |= 1UL << i;
            else Internal &= ~(1UL << i);
        }
    }
        
    #endregion

    public BitBoardIterator GetEnumerator()
    {
        return new BitBoardIterator(Internal, Count);
    }

    public override string ToString()
    {
        string final = "";
        for (int v = 7; v > Board.LBOUND; v--) {
            string bitString = "";
            for (int h = 0; h < Board.UBOUND; h++) {
                bitString += (this[v * 8 + h] ? 1 : "*") + " ";
            }

            final += bitString + "\n";
        }

        return final;
    }

}

public ref struct BitBoardIterator
{
        
    private readonly int Count;

    private ulong Value;
    private int Iteration;

    public BitBoardIterator(ulong value, int count)
    {
        Value = value;
        Count = count;
        Iteration = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext()
    {
        Iteration++;
        return Iteration <= Count;
    }

    public Square Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            int i = BitOperations.TrailingZeroCount(Value);
                
            // Subtract 1 and only hold set bits in that mask.
            Value &= Value - 1;

            return (Square)i;
        }
    }

}
using System.Numerics;
using System.Runtime.CompilerServices;
using Backend.Data.Enum;

namespace Backend.Data.Struct
{

    public struct BitBoard
    {

        public static readonly BitBoard Default = new(ulong.MinValue);
        public static readonly BitBoard[] Hs = {
            0x101010101010101,
            0x202020202020202,
            0x404040404040404,
            0x808080808080808,
            0x1010101010101010,
            0x2020202020202020,
            0x4040404040404040,
            0x8080808080808080
        };
        public static readonly BitBoard[] Vs = {
            0xFF, 
            0xFF00, 
            0xFF0000, 
            0xFF000000,
            0xFF00000000,
            0xFF0000000000,
            0xFF000000000000,
            0xFF00000000000000
        };
        public static readonly BitBoard Edged = Hs[0] | Hs[7] | Vs[0] | Vs[7];

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
        
        public BitBoard(BitBoard from)
        {
            Internal = from.Internal;
        }

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

        public override bool Equals(object obj)
        {
            return obj is BitBoard other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Internal.GetHashCode();
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

        private bool Equals(BitBoard other)
        {
            return Internal == other.Internal;
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

}
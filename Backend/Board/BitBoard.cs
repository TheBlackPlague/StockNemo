using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace Backend.Board
{

    public struct BitBoard : IEnumerable<(int, int)>
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

        private static readonly int[][] OneD = {
            new [] {0, 1, 2, 3, 4, 5, 6, 7},
            new [] {8, 9, 10, 11, 12, 13, 14, 15},
            new [] {16, 17, 18, 19, 20, 21, 22, 23},
            new [] {24, 25, 26, 27, 28, 29, 30, 31},
            new [] {32, 33, 34, 35, 36, 37, 38, 39},
            new [] {40, 41, 42, 43, 44, 45, 46, 47},
            new [] {48, 49, 50, 51, 52, 53, 54, 55},
            new [] {56, 57, 58, 59, 60, 61, 62, 63}
        };
        
        public int Count => BitOperations.PopCount(Internal); // Number of set bits.

        private ulong Internal;

        public static BitBoard operator +(BitBoard left, BitBoard right)
        {
            left.Internal += right.Internal;
            return left;
        }
        
        public static BitBoard operator -(BitBoard left, BitBoard right)
        {
            left.Internal -= right.Internal;
            return left;
        }
        
        public static BitBoard operator *(BitBoard left, BitBoard right)
        {
            left.Internal *= right.Internal;
            return left;
        }

        public static BitBoard operator /(BitBoard left, BitBoard right)
        {
            left.Internal /= right.Internal;
            return left;
        }

        public static BitBoard operator %(BitBoard to, ulong by)
        {
            to.Internal %= by;
            return to;
        }

        public static BitBoard operator |(BitBoard left, BitBoard right)
        {
            left.Internal |= right.Internal;
            return left;
        }
        
        public static BitBoard operator &(BitBoard left, BitBoard right)
        {
            left.Internal &= right.Internal;
            return left;
        }
        
        public static BitBoard operator ~(BitBoard bitBoard)
        {
            bitBoard.Internal = ~bitBoard.Internal;
            return bitBoard;
        }

        public static BitBoard operator >>(BitBoard bitBoard, int by)
        {
            bitBoard.Internal >>= by;
            return bitBoard;
        }
        
        public static BitBoard operator <<(BitBoard bitBoard, int by)
        {
            bitBoard.Internal <<= by;
            return bitBoard;
        }

        public static bool operator ==(BitBoard left, BitBoard right)
        {
            return left.Internal == right.Internal;
        }
        
        public static bool operator !=(BitBoard left, BitBoard right)
        {
            return left.Internal != right.Internal;
        }

        public static implicit operator bool(BitBoard bitBoard)
        {
            return bitBoard.Internal != 0UL;
        }
        
        public static implicit operator ulong(BitBoard bitBoard)
        {
            return bitBoard.Internal;
        }

        public static implicit operator BitBoard(ulong from)
        {
            return new BitBoard(from);
        }

        public static implicit operator BitBoard((int, int) from)
        {
            BitBoard a = Default;
            a[from.Item1, from.Item2] = true;
            return a;
        }

        public static explicit operator (int, int)(BitBoard bitBoard)
        {
            if (bitBoard.Count != 1) 
                throw new InvalidOperationException("Cannot convert this bitboard to tuple.");

            ulong i = 1UL;
            ulong copy = bitBoard.Internal;

            int p = 0;
            while ((i & copy) == 0) {
                i <<= 1;
                ++p;
            }

            int v = p / 8;
            return (p - v * 8, v);
        }
        
        public BitBoard(BitBoard from)
        {
            Internal = from.Internal;
        }

        private BitBoard(ulong from)
        {
            Internal = from;
        }

        public bool this[int h, int v]
        {
            get => (Internal >> OneD[v][h] & 1UL) == 1UL;
            set
            {
                if (value) Internal |= 1UL << OneD[v][h];
                else Internal &= ~(1UL << OneD[v][h]);
            }
        }

        public IEnumerator<(int, int)> GetEnumerator()
        {
            return new BitBoardEnumerator(Internal, Count);
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
            for (int v = 7; v > DataBoard.LBOUND; v--) {
                string bitString = "";
                for (int h = 0; h < DataBoard.UBOUND; h++) {
                    bitString += (this[h, v] ? 1 : "*") + " ";
                }

                final += bitString + "\n";
            }

            return final;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private bool Equals(BitBoard other)
        {
            return Internal == other.Internal;
        }

    }

    public class BitBoardEnumerator : IEnumerator<(int, int)>
    {
        
        private readonly int Count;

        private ulong Value;
        private int Iteration;

        public BitBoardEnumerator(ulong value, int count)
        {
            Value = value;
            Count = count;
        }

        public bool MoveNext()
        {
            Iteration++;
            return Iteration <= Count;
        }

        public void Reset()
        {
            Iteration = 0;
        }

        object IEnumerator.Current => Current;

        public (int, int) Current
        {
            get
            {
                int i = BitOperations.TrailingZeroCount(Value);
                Value &= Value - 1;

                int v = i / 8;
                return (i - v * 8, v);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

    }

}
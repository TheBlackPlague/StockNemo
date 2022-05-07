using System;
using System.Numerics;

namespace Backend.Board
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
        
        public int Count => BitOperations.PopCount(Internal); // Number of set bits.

        private ulong Internal;

        public static BitBoard operator +(BitBoard left, BitBoard right)
        {
            return new BitBoard(left.Internal + right.Internal);
        }
        
        public static BitBoard operator -(BitBoard left, BitBoard right)
        {
            return new BitBoard(left.Internal - right.Internal);
        }
        
        public static BitBoard operator *(BitBoard left, BitBoard right)
        {
            return new BitBoard(left.Internal * right.Internal);
        }

        public static BitBoard operator /(BitBoard left, BitBoard right)
        {
            return new BitBoard(left.Internal / right.Internal);
        }

        public static BitBoard operator %(BitBoard to, ulong by)
        {
            return new BitBoard(to.Internal % by);
        }

        public static BitBoard operator |(BitBoard left, BitBoard right)
        {
            return new BitBoard(left.Internal | right.Internal);
        }
        
        public static BitBoard operator &(BitBoard left, BitBoard right)
        {
            return new BitBoard(left.Internal & right.Internal);
        }
        
        public static BitBoard operator ~(BitBoard bitBoard)
        {
            return new BitBoard(~bitBoard.Internal);
        }

        public static BitBoard operator >>(BitBoard bitBoard, int by)
        {
            return new BitBoard(bitBoard.Internal >> by);
        }
        
        public static BitBoard operator <<(BitBoard bitBoard, int by)
        {
            return new BitBoard(bitBoard.Internal << by);
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
            return new BitBoard(Default)
            {
                [from.Item1, from.Item2] = true 
            };
        }

        public static explicit operator (int, int)(BitBoard bitBoard)
        {
            if (bitBoard.Count != 1) 
                throw new InvalidOperationException("Cannot convert this bitboard to tuple.");

            ulong i = 1;
            ulong copy = bitBoard.Internal;

            int p = 1;
            while ((i & copy) == 0) {
                i <<= 1;
                ++p;
            }

            return (p % 8, p / 8);
        }
        
        private static int OneD(int h, int v)
        {
            if (h is > 7 or < 0 || v is > 7 or < 0) 
                throw new InvalidOperationException("Invalid index: " + (h, v));
            return v * 8 + h;
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
            get => (Internal >> OneD(h, v) & 1UL) == 1UL;
            set
            {
                if (value) Internal |= 1UL << OneD(h, v);
                else Internal &= ~(1UL << OneD(h, v));
            }
        }

        public override bool Equals(object obj)
        {
            return obj is BitBoard other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Internal.GetHashCode();
        }

        public BitBoard Clone()
        {
            return new BitBoard(this);
        }

        public override string ToString()
        {
            string final = "";
            for (int v = 7; v > BitDataBoard.LBOUND; v--) {
                string bitString = "";
                for (int h = 0; h < BitDataBoard.UBOUND; h++) {
                    bitString += (this[h, v] ? 1 : "*") + " ";
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

}
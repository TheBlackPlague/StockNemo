using System;

namespace Backend.Board
{

    public struct BitBoard
    {

        public static readonly BitBoard Default = new(ulong.MinValue);

        private ulong Internal;

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
        
        private static int OneD(int h, int v)
        {
            if (h > 7 || v > 7) throw new InvalidOperationException("Invalid index: " + (h, v));
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

        public int Count()
        {
            ulong c = 0;
            while (Internal > 0) {
                c += Internal & 1UL;
                Internal >>= 1;
            }

            return (int)c;
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
        
        private bool Equals(BitBoard other)
        {
            return Internal == other.Internal;
        }

    }

}
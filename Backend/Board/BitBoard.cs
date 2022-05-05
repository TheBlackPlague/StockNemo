using System;

namespace Backend.Board
{

    public struct BitBoard
    {

        public static readonly BitBoard Default = new(ulong.MinValue);

        private ulong Internal;
        
        private static int OneD(int h, int v)
        {
            if (h > 7 || v > 7) throw new InvalidOperationException("Invalid index: " + (h, v));
            return v * 8 + h;
        }
        
        public BitBoard(BitBoard from)
        {
            Internal = from.Internal;
        }

        public BitBoard(ulong from)
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

        public BitBoard Or(BitBoard second)
        {
            return new BitBoard(Internal | second.Internal);
        }

        public BitBoard And(BitBoard second)
        {
            return new BitBoard(Internal & second.Internal);
        }

        public BitBoard Flip()
        {
            return new BitBoard(~Internal);
        }

        public bool True()
        {
            return Internal != 0UL;
        }

        public bool Equals(BitBoard second)
        {
            return Internal == second.Internal;
        }

        public BitBoard Clone()
        {
            return new BitBoard(this);
        }

    }

}
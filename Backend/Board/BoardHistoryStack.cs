using System;

namespace Backend.Board
{

    public struct BoardHistoryStack
    {

        public int Count;
        private readonly BitBoardMap[] Internal;

        public BoardHistoryStack(int size)
        {
            Internal = GC.AllocateUninitializedArray<BitBoardMap>(size);
            Count = 0;
        }

        private BoardHistoryStack(BoardHistoryStack stack)
        {
            Count = stack.Count;
            Internal = GC.AllocateUninitializedArray<BitBoardMap>(stack.Internal.Length);
            Array.Copy(stack.Internal, Internal, Count);
        }

        public void Push(BitBoardMap element) => Internal[Count++] = element;

        public BitBoardMap Pop() => Internal[--Count];

        public BoardHistoryStack Clone() => new(this);

    }

}
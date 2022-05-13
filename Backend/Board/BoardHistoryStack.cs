using Backend.Move;

namespace Backend.Board
{

    public struct BoardHistoryStack
    {

        public int Count;
        
        private readonly (BoardState, MoveState)[] Internal;

        public BoardHistoryStack(int size)
        {
            Internal = new (BoardState, MoveState)[size];
            Count = 0;
        }

        public void Push((BoardState, MoveState) element) => Internal[Count++] = element;

        public (BoardState, MoveState) Pop() => Internal[--Count];

    }

}
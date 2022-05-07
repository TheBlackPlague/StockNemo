using System;
using Backend.Board;
using Backend.Move;
using NUnit.Framework;

namespace Test
{

    public class BitLegalMoveSetTestUnit
    {

        private readonly BitDataBoard Board = BitDataBoard.Default();

        [Test]
        public void CountKnightMovesAtB1()
        {
            BitLegalMoveSet moveSet = new(Board, (1, 0));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(2, moveSet.Count);
        }
        
        [Test]
        public void CountPawnMovesAtA2()
        {
            BitLegalMoveSet moveSet = new(Board, (0, 1));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(2, moveSet.Count);
        }
        
        [Test]
        public void CountKnightMovesAtA3()
        {
            BitDataBoard use = Board.Clone();
            use.Move((1, 0), (0, 2));
            BitLegalMoveSet moveSet = new(use, (0, 2));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(3, moveSet.Count);
        }

    }

}
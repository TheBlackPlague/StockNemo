using System;
using Backend.Board;
using Backend.Move;
using NUnit.Framework;

namespace Test
{

    public class LegalMoveSetTestUnit
    {

        private readonly DataBoard Board = DataBoard.Default();

        [SetUp]
        public void SetUp()
        {
            LegalMoveSet.SetUp();
        }

        [Test]
        public void CountKnightMovesAtB1()
        {
            LegalMoveSet moveSet = new(Board, (1, 0));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(2, moveSet.Count);
        }
        
        [Test]
        public void CountPawnMovesAtA2()
        {
            LegalMoveSet moveSet = new(Board, (0, 1));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(2, moveSet.Count);
        }
        
        [Test]
        public void CountKnightMovesAtA3()
        {
            DataBoard use = Board.Clone();
            use.Move((1, 0), (0, 2));
            LegalMoveSet moveSet = new(use, (0, 2));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(3, moveSet.Count);
        }
        
        [Test]
        public void CountRookMovesAtA3()
        {
            DataBoard use = Board.Clone();
            use.Move((0, 0), (0, 2));
            LegalMoveSet moveSet = new(use, (0, 2));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(11, moveSet.Count);
        }
        
        [Test]
        public void CountRookMovesAtA1()
        {
            DataBoard use = Board.Clone();
            LegalMoveSet moveSet = new(use, (0, 0));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(0, moveSet.Count);
        }
        
        [Test]
        public void CountBishopMovesAtC3()
        {
            DataBoard use = Board.Clone();
            use.Move((2, 0), (2, 2));
            LegalMoveSet moveSet = new(use, (2, 2));
            Console.WriteLine(moveSet.Get().ToString());
            Assert.AreEqual(6, moveSet.Count);
        }

    }

}
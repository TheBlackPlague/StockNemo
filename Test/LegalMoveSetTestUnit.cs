using System;
using Backend;
using Backend.Data.Move;
using Backend.Data.Struct;
using NUnit.Framework;

namespace Test
{

    public class LegalMoveSetTestUnit
    {

        private readonly Board Board = Board.Default();

        [SetUp]
        public void SetUp()
        {
            MoveList.SetUp();
        }

        [Test]
        public void CountKnightMovesAtB1()
        {
            MoveList moveList = new(Board, (1, 0));
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(2, moveList.Count);
        }
        
        [Test]
        public void CountPawnMovesAtA2()
        {
            MoveList moveList = new(Board, (0, 1));
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(2, moveList.Count);
        }
        
        [Test]
        public void CountKnightMovesAtA3()
        {
            Board use = Board.Clone();
            use.Move((1, 0), (0, 2));
            MoveList moveList = new(use, (0, 2));
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(3, moveList.Count);
        }
        
        [Test]
        public void CountRookMovesAtA3()
        {
            Board use = Board.Clone();
            use.Move((0, 0), (0, 2));
            MoveList moveList = new(use, (0, 2));
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(11, moveList.Count);
        }
        
        [Test]
        public void CountRookMovesAtA1()
        {
            Board use = Board.Clone();
            MoveList moveList = new(use, (0, 0));
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(0, moveList.Count);
        }
        
        [Test]
        public void CountBishopMovesAtC3()
        {
            Board use = Board.Clone();
            use.Move((2, 0), (2, 2));
            MoveList moveList = new(use, (2, 2));
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(6, moveList.Count);
        }

    }

}
﻿using System;
using Backend;
using Backend.Data.Enum;
using NUnit.Framework;

namespace Test
{

    public class MoveList
    {

        private readonly Board Board = Board.Default();

        [SetUp]
        public void SetUp()
        {
            Backend.Data.Struct.MoveList.SetUp();
        }

        [Test]
        public void CountKnightMovesAtB1()
        {
            Backend.Data.Struct.MoveList moveList = new(Board, Square.B1);
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(2, moveList.Count);
        }
        
        [Test]
        public void CountPawnMovesAtA2()
        {
            Backend.Data.Struct.MoveList moveList = new(Board, Square.A2);
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(2, moveList.Count);
        }
        
        [Test]
        public void CountKnightMovesAtA3()
        {
            Board use = Board.Clone();
            use.Move(Square.B1, Square.A3);
            Backend.Data.Struct.MoveList moveList = new(use, Square.A3);
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(3, moveList.Count);
        }
        
        [Test]
        public void CountRookMovesAtA3()
        {
            Board use = Board.Clone();
            use.Move(Square.A1, Square.A3);
            Backend.Data.Struct.MoveList moveList = new(use, Square.A3);
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(11, moveList.Count);
        }
        
        [Test]
        public void CountRookMovesAtA1()
        {
            Board use = Board.Clone();
            Backend.Data.Struct.MoveList moveList = new(use, Square.A1);
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(0, moveList.Count);
        }
        
        [Test]
        public void CountBishopMovesAtC3()
        {
            Board use = Board.Clone();
            use.Move(Square.C1, Square.C3);
            Backend.Data.Struct.MoveList moveList = new(use, Square.C3);
            Console.WriteLine(moveList.Get().ToString());
            Assert.AreEqual(6, moveList.Count);
        }

    }

}
using Backend.Data.Enum;
using Backend.Data.Struct;
using NUnit.Framework;

namespace Test
{

    public class BitBoardMapTestUnit
    {

        private readonly BitBoardMap Map = 
            new("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR", "w", "KQkq", "-");

        [Test]
        public void Clone() => Assert.AreEqual(Map, Map);
        
        [Test]
        public void GetWhitePawn() => Assert.AreEqual((Piece.Pawn, PieceColor.White), Map[0, 1]);
        
        [Test]
        public void GetWhiteRook() => Assert.AreEqual((Piece.Rook, PieceColor.White), Map[0, 0]);
        
        [Test]
        public void GetWhiteQueen() => Assert.AreEqual((Piece.Queen, PieceColor.White), Map[3, 0]);
        
        [Test]
        public void GetWhiteKing() => Assert.AreEqual((Piece.King, PieceColor.White), Map[4, 0]);
        
        [Test]
        public void GetBlackPawn() => Assert.AreEqual((Piece.Pawn, PieceColor.Black), Map[0, 6]);
        
        [Test]
        public void GetBlackRook() => Assert.AreEqual((Piece.Rook, PieceColor.Black), Map[0, 7]);
        
        [Test]
        public void GetBlackQueen() => Assert.AreEqual((Piece.Queen, PieceColor.Black), Map[3, 7]);
        
        [Test]
        public void GetBlackKing() => Assert.AreEqual((Piece.King, PieceColor.Black), Map[4, 7]);

        [Test]
        public void MoveWhitePawn()
        {
            BitBoardMap useMap = Map;
            
            useMap.Move((0, 1), (0, 3));
            Assert.AreEqual((Piece.Pawn, PieceColor.White), useMap[0, 3]);
        }
        
        [Test]
        public void MoveWhitePawnInEnemy()
        {
            BitBoardMap useMap = Map;
            
            useMap.Move((0, 1), (0, 6));

            bool success = (Piece.Pawn, PieceColor.White) == useMap[0, 6] &&
                           useMap[Piece.Pawn, PieceColor.Black].Count == 7;
            Assert.IsTrue(success);
        }
        
        [Test]
        public void RemoveWhitePawn()
        {
            BitBoardMap useMap = Map;
            
            useMap.Empty(0, 1);
            Assert.AreEqual((Piece.Empty, PieceColor.None), useMap[0, 1]);
        }
        
        [Test]
        public void MoveKnightToA3()
        {
            BitBoardMap useMap = Map;
            
            useMap.Move((1, 0), (0, 2));
            Assert.AreEqual((Piece.Knight, PieceColor.White), useMap[0, 2]);
        }

    }

}
// using System;
// using Backend.Data.Struct;
// using NUnit.Framework;
//
// namespace Test
// {
//
//     public class BitBoardTestUnit
//     {
//
//         [Test]
//         public void TestDefault()
//         {
//             bool success = true;
//             for (int h = 0; h < 8; h++)
//             for (int v = 0; v < 8; v++) {
//                 bool lookup = !BitBoard.Default[h, v];
//                 if (lookup) continue;
//                 
//                 success = false;
//                 goto AssertionTest;
//             }
//             
//             AssertionTest:
//             Assert.IsTrue(success);
//         }
//
//         [Test]
//         public void Mark0X0AsTrue()
//         {
//             BitBoard useBoard = new(BitBoard.Default)
//             {
//                 [0, 0] = true
//             };
//             
//             Assert.IsTrue(useBoard[0, 0]);
//         }
//
//         [Test]
//         public void MarkWhiteAsTrue()
//         {
//             BitBoard useBoard = new(BitBoard.Default);
//             
//             for (int h = 0; h < 8; h++)
//             for (int v = 0; v < 8; v++) {
//                 if (v < 2) useBoard[h, v] = true;
//             }
//
//             bool success = true;
//             for (int h = 0; h < 8; h++)
//             for (int v = 0; v < 8; v++) {
//                 if (v < 2) {
//                     if (useBoard[h, v]) continue;
//                     success = false;
//                     goto AssertionTest;
//                 }
//
//                 if (!useBoard[h, v]) continue;
//                 success = false;
//                 goto AssertionTest;
//             }
//             
//             AssertionTest:
//             Assert.IsTrue(success);
//         }
//         
//         [Test]
//         public void MarkBlackAndWhiteAsTrue()
//         {
//             BitBoard whiteBoard = new(BitBoard.Default);
//             BitBoard blackBoard = new(BitBoard.Default);
//             
//             for (int h = 0; h < 8; h++)
//             for (int v = 0; v < 8; v++) {
//                 switch (v) {
//                     case < 2:
//                         whiteBoard[h, v] = true;
//                         break;
//                     case > 5:
//                         blackBoard[h, v] = true;
//                         break;
//                 }
//             }
//
//             BitBoard final = whiteBoard | blackBoard;
//
//             bool success = true;
//             for (int h = 0; h < 8; h++)
//             for (int v = 0; v < 8; v++) {
//                 switch (v) {
//                     case < 2:
//                     case > 5:
//                         if (!final[h, v]) {
//                             success = false;
//                             goto AssertionTest;
//                         }
//                         break;
//                     default:
//                         if (final[h, v]) {
//                             success = false;
//                             goto AssertionTest;
//                         }
//                         break;
//                 }
//             }
//             
//             AssertionTest:
//             Assert.IsTrue(success);
//         }
//
//         [Test]
//         public void InvalidIndex()
//         {
//             bool success = false;
//             try {
//                 bool unused = BitBoard.Default[8, 0];
//             } catch (IndexOutOfRangeException) {
//                 success = true;
//             }
//             
//             Assert.IsTrue(success);
//         }
//         
//     }
//
// }
using Backend.Data.Enum;
using NUnit.Framework;

namespace Test;

public class BitBoard
{

    [Test]
    public void TestDefault()
    {
        bool success = true;
        for (int h = 0; h < 8; h++)
        for (int v = 0; v < 8; v++) {
            bool lookup = !Backend.Data.Struct.BitBoard.Default[v * 8 + h];
            if (lookup) continue;
                
            success = false;
            goto AssertionTest;
        }
            
        AssertionTest:
        Assert.IsTrue(success);
    }

    [Test]
    public void MarkA1AsTrue()
    {
        Backend.Data.Struct.BitBoard useBoard = new(Backend.Data.Struct.BitBoard.Default);
        useBoard.True(Square.A1);
            
        Assert.IsTrue(useBoard[Square.A1]);
    }

    [Test]
    public void MarkWhiteAsTrue()
    {
        Backend.Data.Struct.BitBoard useBoard = new(Backend.Data.Struct.BitBoard.Default);
            
        for (int h = 0; h < 8; h++)
        for (int v = 0; v < 8; v++) {
            if (v < 2) useBoard.True(v * 8 + h);
        }

        bool success = true;
        for (int h = 0; h < 8; h++)
        for (int v = 0; v < 8; v++) {
            if (v < 2) {
                if (useBoard[v * 8 + h]) continue;
                success = false;
                goto AssertionTest;
            }

            if (!useBoard[v * 8 + h]) continue;
            success = false;
            goto AssertionTest;
        }
            
        AssertionTest:
        Assert.IsTrue(success);
    }
        
    [Test]
    public void MarkBlackAndWhiteAsTrue()
    {
        Backend.Data.Struct.BitBoard whiteBoard = new(Backend.Data.Struct.BitBoard.Default);
        Backend.Data.Struct.BitBoard blackBoard = new(Backend.Data.Struct.BitBoard.Default);
            
        for (int h = 0; h < 8; h++)
        for (int v = 0; v < 8; v++) {
            switch (v) {
                case < 2:
                    whiteBoard.True(v * 8 + h);
                    break;
                case > 5:
                    blackBoard.True(v * 8 + h);
                    break;
            }
        }

        Backend.Data.Struct.BitBoard final = whiteBoard | blackBoard;

        bool success = true;
        for (int h = 0; h < 8; h++)
        for (int v = 0; v < 8; v++) {
            switch (v) {
                case < 2:
                case > 5:
                    if (!final[v * 8 + h]) {
                        success = false;
                        goto AssertionTest;
                    }
                    break;
                default:
                    if (final[v * 8 + h]) {
                        success = false;
                        goto AssertionTest;
                    }
                    break;
            }
        }
            
        AssertionTest:
        Assert.IsTrue(success);
    }
        
}
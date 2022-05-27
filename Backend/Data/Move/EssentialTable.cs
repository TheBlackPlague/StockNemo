using System;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend.Data.Move;

public static class EssentialTable
{
    
    public static readonly BitBoard[] Hs = {
        0x101010101010101,
        0x202020202020202,
        0x404040404040404,
        0x808080808080808,
        0x1010101010101010,
        0x2020202020202020,
        0x4040404040404040,
        0x8080808080808080
    };
    public static readonly BitBoard[] Vs = {
        0xFF, 
        0xFF00, 
        0xFF0000, 
        0xFF000000,
        0xFF00000000,
        0xFF0000000000,
        0xFF000000000000,
        0xFF00000000000000
    };
    public static readonly BitBoard Edged = Hs[0] | Hs[7] | Vs[0] | Vs[7];

    public static readonly BitBoard[][] Between = new BitBoard[64][];

    public static void GenerateBetweenTable()
    {
        for (Square fromSq = Square.A1; fromSq < Square.Na; fromSq++) {
            (int fromH, int fromV) = ((int)fromSq % 8, (int)fromSq / 8);
            Between[(int)fromSq] = new BitBoard[64];

            for (Square toSq = Square.A1; toSq < Square.Na; toSq++) {
                Between[(int)fromSq][(int)toSq] = BitBoard.Default;
                // It's the same square so we can skip.
                if (fromSq == toSq) continue;
                
                (int toH, int toV) = ((int)toSq % 8, (int)toSq / 8);

                if (fromH == toH || fromV == toV) {
                    // We calculate rook (straight) squares here.

                    Square high;
                    Square low;
                    int delta;
                    
                    if (fromH != toH) {
                        // We have a horizontal delta.
                        high = fromSq > toSq ? fromSq : toSq;
                        low = high == fromSq ? toSq : fromSq;
                        delta = 1;
                    } else {
                        // We have a vertical delta.
                        high = fromSq > toSq ? fromSq : toSq;
                        low = high == fromSq ? toSq : fromSq;
                        delta = 8;
                    }
                    
                    for (Square sq = high - delta; sq > low; sq -= delta) Between[(int)fromSq][(int)toSq][sq] = true;
                    
                    // No need to calculate abs values.
                    continue;
                }

                int absH = Math.Abs(fromH - toH);
                int absV = Math.Abs(fromV - toV);
                if (absH != absV) continue;

                // We calculate bishop (diagonal) squares between here.
                int[] highs = {
                    fromH > toH ? fromH : toH,
                    fromV > toV ? fromV : toV
                };
                int[] lows = {
                    highs[0] == fromH ? toH : fromH,
                    highs[1] == fromV ? toV : fromV
                };
                    
                for (int h = highs[0] - 1; h > lows[0]; h--)
                for (int v = highs[1] - 1; v > lows[1]; v--) {
                    if (h != v) continue;
                    
                    int sq = v * 8 + h;
                    Between[(int)fromSq][(int)toSq][sq] = true;
                }
            }
        }
        
        Console.WriteLine(Between[(int)Square.A1][(int)Square.A8].ToString());
        Console.WriteLine(Between[(int)Square.A1][(int)Square.H8].ToString());
        Console.WriteLine(Between[(int)Square.A1][(int)Square.H1].ToString());
    }

}
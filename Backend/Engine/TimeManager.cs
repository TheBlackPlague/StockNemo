using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Backend.Data.Template;

namespace Backend.Engine;

public static class TimeManager
{

    private const int COMPLEXITY_THRESHOLD = 400;
    private const int BASE_TIME_FACTOR = 20;
    private const int INCREMENT_TIME_FACTOR = 2;
    
    public static CancellationToken ThreadToken => Internal.Token;

    private static CancellationTokenSource Internal = new();
    private static bool CurrentlySetup;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Setup(int time = -1)
    {
        Reset();
        
        if (time != -1) Internal.CancelAfter(time);
        
        CurrentlySetup = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Setup(EngineBoard board, ReadOnlySpan<int> timeLeft, ReadOnlySpan<int> timeIncrement, 
        int movesToGo = -1)
    {
        Reset();

        // PieceColor opponentColor = board.ColorToMove.OppositeColor();
        int ourTime = timeLeft[(int)board.ColorToMove];
        // int opponentTime = timeLeft[(int)opponentColor];
        int ourIncrement = timeIncrement[(int)board.ColorToMove];
        // int opponentIncrement = timeIncrement[(int)opponentColor];

        int time = ourTime / BASE_TIME_FACTOR;
        
        if (movesToGo != -1) time = Math.Max(time, (ourTime + ourIncrement) / movesToGo);

        time += ourIncrement / INCREMENT_TIME_FACTOR;

        Internal.CancelAfter(time);

        CurrentlySetup = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ChangeTime(int time)
    {
        Internal.CancelAfter(time);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OutOfTime() => ThreadToken.IsCancellationRequested;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Reset()
    {
        if (CurrentlySetup) {
            Internal = new CancellationTokenSource();
            CurrentlySetup = false;
        }
    }
    
}
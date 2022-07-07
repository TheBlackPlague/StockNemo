using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Backend.Data.Enum;

namespace Backend.Engine;

public class TimeControl
{
    
    private const int INCREMENT_MOVE_BOUND = 10;
    private const int DELTA_MOVE_BOUND = 20;
    private const int DELTA_THRESHOLD = 3000;

    private const int BASE_DIV = 20;
    private const int INCREMENT_DIV = 2;
    private const int DELTA_DIV = 3;

    public readonly CancellationToken Token;
    public int Time { get; private set; }

    private readonly long StartTime;
    private readonly CancellationTokenSource Source = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long GetCurrentTime() => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeControl(ReadOnlySpan<int> timeForColor, ReadOnlySpan<int> timeIncForColor, 
        PieceColor colorToMove, int moveCount)
    {
        Time = timeForColor[(int)colorToMove] / BASE_DIV;

        if (moveCount >= INCREMENT_MOVE_BOUND) Time += timeIncForColor[(int)colorToMove] / INCREMENT_DIV;

        // ReSharper disable once InvertIf
        if (moveCount >= DELTA_MOVE_BOUND) {
            int dTime = timeForColor[(int)colorToMove] - timeForColor[(int)Util.OppositeColor(colorToMove)];
            if (dTime >= DELTA_THRESHOLD) Time += dTime / DELTA_DIV;
        }
        
        Source.CancelAfter(Time);
        Token = Source.Token;
        StartTime = GetCurrentTime();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeControl(int time)
    {
        Time = time;
        Source.CancelAfter(Time);
        Token = Source.Token;
        StartTime = GetCurrentTime();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Finished() => Token.IsCancellationRequested;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int TimeLeft() => Time - ElapsedTime();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ChangeTime(int time)
    {
        if (time - ElapsedTime() <= 0) {
            Source.Cancel();
            return;
        }

        Time = time;
        Source.CancelAfter(TimeLeft());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int ElapsedTime() => (int)(GetCurrentTime() - StartTime);

}
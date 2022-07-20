using System;
using Version = Backend.Version;

namespace Terminal;

internal static class DrawCycle
{

    public static void OutputTitle()
    {
        Console.WriteLine("StockNemo v" + Version.Get() + "\nCopyright (c) Shaheryar Sohail. All rights reserved.");
    }

    public static void Draw(DisplayBoard board)
    {
        Console.Clear();
        OutputTitle();
        HardwareInfoDisplay display = HardwareInitializer.Display();
        
        Console.WriteLine("┌───────────────────┐  ┌" + display.CpuDash + "┐");
        Console.WriteLine("│  Chess Board CLI  │  │  " + display.CpuName + "  │");
        Console.WriteLine("└───────────────────┘  └" + display.CpuDash + "┘");
        
        Console.WriteLine(board.ToString());
    }

}
using System;
using System.Collections.Generic;

namespace Terminal.Interactive;

public class Menu
{

    private readonly string Text;
    private readonly Action Prepend;
    private readonly List<Option> Options;
    
    private int CursorPosition;

    public Menu(string text, Action prepend, params Option[] options)
    {
        Text = text;
        Prepend = prepend;
        Options = new List<Option>(options);
    }

    public void Display()
    {
        Console.Clear();
        Prepend.Invoke();
        Console.WriteLine("");
        Console.WriteLine(Text);
        Console.WriteLine("");
        Console.WriteLine("Press ESC to exit menu.");
        Console.WriteLine("");

        int i = 0;
        ConsoleColor defaultColor = Console.ForegroundColor;
        foreach (Option option in Options) {
            if (i == CursorPosition) {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("> ");
            } else {
                Console.ForegroundColor = defaultColor;
                Console.Write("  ");
            }
            Console.WriteLine(option.Text);
            i++;
        }
        Console.ForegroundColor = defaultColor;
    }

    public void ListenForCursorUpdate()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey();
        while (keyInfo.Key != ConsoleKey.Escape) {
            int previousCursorPosition = CursorPosition;

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
            switch (keyInfo.Key) {
                case ConsoleKey.DownArrow:
                    CursorPosition++;
                    break;
                case ConsoleKey.UpArrow:
                    CursorPosition--;
                    break;
                case ConsoleKey.Enter:
                    Options[CursorPosition].OnClick.Invoke();
                    return;
            }

            // ReSharper disable once InvertIf
            if (CursorPosition != previousCursorPosition) {
                if (CursorPosition < 0) CursorPosition = Options.Count - 1;
                if (CursorPosition > Options.Count - 1) CursorPosition = 0;
                
                Display();
            }
            
            keyInfo = Console.ReadKey();
        }
    }

}
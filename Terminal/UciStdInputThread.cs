using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal;

public static class UciStdInputThread
{

#pragma warning disable CA2211
    public static EventHandler<string> CommandReceived;
    public static bool Running;
#pragma warning restore CA2211

    public static void StartAcceptingInput()
    {
        Running = true;

        while (Running) {
            string input = Console.ReadLine();
            if (input!.ToLower().Equals("exit_hard")) return;
            CommandReceived.Invoke(Thread.CurrentThread, input);
        }
    }

}
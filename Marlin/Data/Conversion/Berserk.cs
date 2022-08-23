using System.Runtime.CompilerServices;
using System.Text;

namespace Marlin.Data.Conversion;

public class Berserk : IEngine
{

    private const int PARALLELISM = 1024;
    private const int BATCH = 2048 * 10000;

    private readonly string[] Buffer = new string[BATCH];
    
    private int BufferCount = BATCH - 1;

    private int ConvertedSoFar = 0;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Convert(Stream input, Stream output)
    {
        using StreamReader reader = new(input);
        using StreamWriter writer = new(output);
        ReadOnlySpan<char> separator = stackalloc char[] { ' ', '|', ' ' };

        int batch = 1;
        while (!reader.EndOfStream) {
            FillBufferFromInput(reader);
            (int parallel, int synchronous) = Math.DivRem(BufferCount, PARALLELISM);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void Build(StringBuilder sb, ReadOnlySpan<char> separator, int start, int end)
            {
                for (int j = start; j < end; j++) {
                    ReadOnlySpan<char> remaining = Buffer[j];
                    ReadOnlySpan<char> fen = remaining.Till(' ', 6);
                    remaining = remaining[(fen.Length + 1)..];
                    ReadOnlySpan<char> wdl = remaining[1..4];
                    ReadOnlySpan<char> eval = remaining[6..];

                    sb
                        .Append(fen)
                        .Append(separator)
                        .Append(eval)
                        .Append(separator)
                        .Append(wdl);

                    if (j != end - 1) sb.Append('\n');
                }
            }

            Parallel.For(0, parallel, i =>
            {
                StringBuilder sb = new();
                sb.Append('\n');
                ReadOnlySpan<char> separator = stackalloc char[] { ' ', '|', ' ' };

                int start = i * PARALLELISM;
                int end = start + PARALLELISM;
                Build(sb, separator, start, end);
                
                // ReSharper disable once AccessToDisposedClosure
                lock (writer) {
                    // ReSharper disable once AccessToDisposedClosure
                    writer.Write(sb.ToString());
                }
                
                Interlocked.Add(ref ConvertedSoFar, PARALLELISM);
            });

            StringBuilder sb = new();
            sb.Append('\n');
            int start = parallel * PARALLELISM;
            int end = start + synchronous;
            Build(sb, separator, start, end);
            writer.Write(sb.ToString());
            
            Console.WriteLine("Batch: " + batch + ", Converted: " + ConvertedSoFar);
            batch++;
        }
    }

    private void FillBufferFromInput(StreamReader streamReader)
    {
        int i = 0;
        while (i < BATCH && !streamReader.EndOfStream) {
            Buffer[i] = streamReader.ReadLine()!;
            i++;
        }
        BufferCount = i;
    }

}
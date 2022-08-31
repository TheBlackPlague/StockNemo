using System.Diagnostics;
using System.Runtime.CompilerServices;
using Marlin.Data.Enum;
using Marlin.Data.Struct;

namespace Marlin.Data;

public abstract class FenText : DataImplementation
{

    protected FenText(string path, DataOperation op) : base(path, op) {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract PackedDataPoint PackLine(string line);

    public void Pack(string toPath, int bufferSize = 524288, int parallelism = 1)
    {
        string[] readBuffer = new string[bufferSize];
        PackedDataPoint[] writeBuffer = new PackedDataPoint[bufferSize];
        PackedData packedData = new(toPath, DataOperation.Write);

        using StreamReader reader = new(DataStream);

        bool reachedEnd = false;
        int batch = 1;
        while (reachedEnd == false) {
            Stopwatch stopwatch = Stopwatch.StartNew();
            int converted = Operation();
            stopwatch.Stop();
            
            reachedEnd = converted == 0;
            if (reachedEnd) continue;
            
            double time = stopwatch.Elapsed.TotalMilliseconds;
            Console.WriteLine("Batch [" + batch + "] completed - Speed: " + (converted / time) + " pos/s");
            batch++;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        int Operation()
        {
            // Attempt reading from the file.
            int read = 0;
            for (int i = 0; i < readBuffer.Length; i++) {
                string? line = reader.ReadLine();
                if (line is null) break;
                readBuffer[i] = line;
                read++;
            }

            // If no lines were read, just return.
            if (read == 0) return 0;
            
            // Split the read buffer into chunks for maximum parallelism.
            int chunkSize = read / parallelism;
            
            // In case of imperfect division, some will be left over.
            // 0 <= leftOver < parallelism
            int leftOver = read % parallelism;

            // Convert the data in parallel.
            Parallel.For(0, parallelism, i =>
            {
                int start = i * chunkSize;
                int end = start + chunkSize;
                for (int j = start; j < end; j++) {
                    writeBuffer[j] = PackLine(readBuffer[j]);
                }
            });

            // Convert whatever is left over. This is minimum iterations.
            int start = parallelism * chunkSize;
            int end = start + leftOver;
            for (int i = start; i < end; i++) {
                writeBuffer[i] = PackLine(readBuffer[i]);
            }

            // Write the data to a file.
            for (int i = 0; i < read; i++) {
                writeBuffer[i].WriteToBinary(packedData.DataStream);
            }
            return read;
        }
    }

}
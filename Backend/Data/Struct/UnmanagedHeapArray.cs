using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Backend.Data.Struct;

public unsafe class UnmanagedHeapArray<T> where T : unmanaged
{

    private readonly int Length;
    private readonly IntPtr Internal;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", 
        MessageId = "type: Engine.Data.Struct.MoveTranspositionTableEntry")
    ]
    public UnmanagedHeapArray(int length, bool fill = false)
    {
        Length = length;
        Internal = Marshal.AllocHGlobal(Length * sizeof(T));

        if (!fill) return;
        
        if (length > 0xFF) Parallel.For(0, Length, i =>
        {
            Reference(i) = new T();
        });
        else for (int i = 0; i < Length; i++) {
            Reference(i) = new T();
        }
    }
    
    public ref T this[int index] => ref Reference(index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Reference(int index) => ref Unsafe.Add(ref Reference(), index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Size() => Length * sizeof(T);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Release() => Marshal.FreeHGlobal(Internal);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref T Reference() => ref Unsafe.AsRef<T>((T*)Internal);

}
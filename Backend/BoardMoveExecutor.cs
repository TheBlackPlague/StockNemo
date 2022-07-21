using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend;

public unsafe readonly struct BoardMoveExecutor
{
    private readonly struct True { }

    private static readonly delegate*<ref BitBoardMap, void>* FPs;

    [ModuleInitializer]
    internal static void RunCctor()
    {
        RuntimeHelpers.RunClassConstructor(typeof(BoardMoveExecutor).TypeHandle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static MethodInfo GetMethodInfo(Delegate method)
    {
        return method.Method;
    }

    static BoardMoveExecutor()
    {
        //This should be above, for codegen accuracy
        FPs = (delegate*<ref BitBoardMap, void>*) NativeMemory.Alloc(64, 64);
        
        //This is necessary for InASM
        if (Assembly.GetExecutingAssembly().GetName().Name!.Contains('@'))
        {
            return;
        }
        
        MethodInfo gMethod = GetMethodInfo(ExecuteMove<True, True, True, True, True, True>).GetGenericMethodDefinition();

        const int gArgsCount = 6;
        
        Type[] gArgs = new Type[gArgsCount];

        ref Type firstArgOffsetByOne = ref Unsafe.Subtract(ref MemoryMarshal.GetArrayDataReference(gArgs), 1);

        ref Type lastArg = ref Unsafe.Add(ref firstArgOffsetByOne, gArgsCount);

        for (int I = 0; I < 64; I++)
        {
            const int extractionMask = 1, @true = 1; //LSB set
            
            ref Type currentArg = ref lastArg;

            int id = I;
            
            for (; !Unsafe.AreSame(ref currentArg, ref firstArgOffsetByOne)
                 ; currentArg = ref Unsafe.Subtract(ref currentArg, 1))
            {
                int extractedState = id & extractionMask;

                if (extractedState == @true)
                {
                    currentArg = typeof(True);
                }

                else
                {
                    currentArg = typeof(int); //Any type that isn't True will be treated as false
                }

                id >>= 1;
            }
        
            MethodInfo specializedMethod =  gMethod.MakeGenericMethod(gArgs);

            RuntimeMethodHandle mh = specializedMethod.MethodHandle;
            
            //JIT method before getting FP pointer, allowing FP to point to optimized code
            RuntimeHelpers.PrepareMethod(mh);

            FPs[I] = (delegate*<ref BitBoardMap, void>) mh.GetFunctionPointer();
        }
    }

    //For testing purposes
    public static MethodInfo PopulateGArgsArrayAndCreateSpecializedMethodInstantiation(int id)
    {
        Type[] gArgs = new Type[6];
        
        MethodInfo gMethod = GetMethodInfo(ExecuteMove<True, True, True, True, True, True>).GetGenericMethodDefinition();

        const int gArgsCount = 6;
        
        ref Type firstArgOffsetByOne = ref Unsafe.Subtract(ref MemoryMarshal.GetArrayDataReference(gArgs), 1);

        ref Type currentArg = ref Unsafe.Add(ref firstArgOffsetByOne, gArgsCount);

        const int extractionMask = 1, @true = 1; //LSB set
        
        for (; !Unsafe.AreSame(ref currentArg, ref firstArgOffsetByOne)
             ; currentArg = ref Unsafe.Subtract(ref currentArg, 1))
        {
            int extractedState = id & extractionMask;

            if (extractedState == @true)
            {
                currentArg = typeof(True);
            }

            else
            {
                currentArg = typeof(int); //Any type that isn't True will be treated as false
            }

            id >>= 1;
        }
        
        MethodInfo specializedMethod = gMethod.MakeGenericMethod(gArgs);

        return specializedMethod;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ExecuteMove(ref BitBoardMap map, nint id) //Do NOT reorder params! It will incur an additional reg to reg mov!
    {
        FPs[id](ref map);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void ExecuteMove<TIsWhiteTurn, TIsEnPassantSet, TIsWhiteKCastle, TIsWhiteQCastle, TIsBlackKCastle, TIsBlackQCastle>(ref BitBoardMap map)
    {
        if (typeof(TIsWhiteTurn) == typeof(True))
        {
            Console.WriteLine("IsWhiteTurn");
        }
        
        if (typeof(TIsEnPassantSet) == typeof(True))
        {
            Console.WriteLine("IsEnPassantSet");
        }
        
        if (typeof(TIsWhiteKCastle) == typeof(True))
        {
            Console.WriteLine("IsWhiteKCastle");
        }
        
        if (typeof(TIsWhiteQCastle) == typeof(True))
        {
            Console.WriteLine("IsWhiteQCastle");
        }
        
        if (typeof(TIsBlackKCastle) == typeof(True))
        {
            Console.WriteLine("IsBlackKCastle");
        }
        
        if (typeof(TIsBlackQCastle) == typeof(True))
        {
            Console.WriteLine("IsBlackQCastle");
        }
    }
}
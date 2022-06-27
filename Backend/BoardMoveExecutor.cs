using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Backend.Data;
using Backend.Data.Enum;
using Backend.Data.Struct;

namespace Backend;

public readonly unsafe struct BoardMoveExecutor
{
    private readonly struct True { }

    private static readonly delegate*<ref BitBoardMap, void>* FPs;

    [ModuleInitializer]
    internal static void RunCctor()
    {
        RuntimeHelpers.RunClassConstructor(typeof(BoardMoveExecutor).TypeHandle);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static MethodInfo GetMethodInfo(Delegate Method)
    {
        return Method.Method;
    }

    static BoardMoveExecutor()
    {
        //This should be above, for codegen accuracy
        FPs = (delegate*<ref BitBoardMap, void>*) NativeMemory.Alloc(64, 64);
        
        //This is necessary for InASM
        if (Assembly.GetExecutingAssembly().GetName().Name.Contains('@'))
        {
            return;
        }
        
        var GMethod = GetMethodInfo(ExecuteMove<True, True, True, True, True, True>).GetGenericMethodDefinition();

        const int GArgsCount = 6;
        
        var GArgs = new Type[GArgsCount];

        ref var FirstArgOffsetByOne = ref Unsafe.Subtract(ref MemoryMarshal.GetArrayDataReference(GArgs), 1);

        ref var LastArg = ref Unsafe.Add(ref FirstArgOffsetByOne, GArgsCount);

        for (int I = 0; I < 64; I++)
        {
            const int ExtractionMask = 1, True = 1; //LSB set
            
            ref var CurrentArg = ref LastArg;

            var ID = I;
            
            for (; !Unsafe.AreSame(ref CurrentArg, ref FirstArgOffsetByOne)
                 ; CurrentArg = ref Unsafe.Subtract(ref CurrentArg, 1))
            {
                var ExtractedState = ID & ExtractionMask;

                if (ExtractedState == True)
                {
                    CurrentArg = typeof(True);
                }

                else
                {
                    CurrentArg = typeof(int); //Any type that isn't True will be treated as false
                }

                ID >>= 1;
            }
        
            var SpecializedMethod =  GMethod.MakeGenericMethod(GArgs);

            var MH = SpecializedMethod.MethodHandle;
            
            //JIT method before getting FP pointer, allowing FP to point to optimized code
            RuntimeHelpers.PrepareMethod(MH);

            FPs[I] = (delegate*<ref BitBoardMap, void>) MH.GetFunctionPointer();
        }
    }

    //For testing purposes
    public static MethodInfo PopulateGArgsArrayAndCreateSpecializedMethodInstantiation(int ID)
    {
        var GArgs = new Type[6];
        
        var GMethod = GetMethodInfo(ExecuteMove<True, True, True, True, True, True>).GetGenericMethodDefinition();

        const int GArgsCount = 6;
        
        ref var FirstArgOffsetByOne = ref Unsafe.Subtract(ref MemoryMarshal.GetArrayDataReference(GArgs), 1);

        ref var CurrentArg = ref Unsafe.Add(ref FirstArgOffsetByOne, GArgsCount);

        const int ExtractionMask = 1, True = 1; //LSB set
        
        for (; !Unsafe.AreSame(ref CurrentArg, ref FirstArgOffsetByOne)
             ; CurrentArg = ref Unsafe.Subtract(ref CurrentArg, 1))
        {
            var ExtractedState = ID & ExtractionMask;

            if (ExtractedState == True)
            {
                CurrentArg = typeof(True);
            }

            else
            {
                CurrentArg = typeof(int); //Any type that isn't True will be treated as false
            }

            ID >>= 1;
        }
        
        var SpecializedMethod = GMethod.MakeGenericMethod(GArgs);

        return SpecializedMethod;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static void ExecuteMove(ref BitBoardMap Map, nint ID)
    {
        FPs[ID](ref Map);
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void ExecuteMove<IsWhiteTurn, IsEnPassantSet, IsWhiteKCastle, IsWhiteQCastle, IsBlackKCastle, IsBlackQCastle>(ref BitBoardMap Map)
    {
        if (typeof(IsWhiteTurn) == typeof(True))
        {
            Console.WriteLine("IsWhiteTurn");
        }
        
        if (typeof(IsEnPassantSet) == typeof(True))
        {
            Console.WriteLine("IsEnPassantSet");
        }
        
        if (typeof(IsWhiteKCastle) == typeof(True))
        {
            Console.WriteLine("IsWhiteKCastle");
        }
        
        if (typeof(IsWhiteQCastle) == typeof(True))
        {
            Console.WriteLine("IsWhiteQCastle");
        }
        
        if (typeof(IsBlackKCastle) == typeof(True))
        {
            Console.WriteLine("IsBlackKCastle");
        }
        
        if (typeof(IsBlackQCastle) == typeof(True))
        {
            Console.WriteLine("IsBlackQCastle");
        }
    }
}
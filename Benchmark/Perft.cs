﻿using Backend;
using BenchmarkDotNet.Attributes;

namespace Benchmark
{

    public class Perft
    {

        private readonly Board Default = Backend.Board.Default();
        private readonly Board Kiwipete = 
            Backend.Board.FromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -");
        private readonly Board EpPin = Board.FromFen("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -");
        
        [Benchmark]
        public void PerftDefault1() => Backend.Perft.MoveGeneration(Default, 1, verbose: false);

        [Benchmark]
        public void PerftDefault2() => Backend.Perft.MoveGeneration(Default, 2, verbose: false);
        
        [Benchmark]
        public void PerftDefault3() => Backend.Perft.MoveGeneration(Default, 3, verbose: false);
        
        [Benchmark]
        public void PerftDefault4() => Backend.Perft.MoveGeneration(Default, 4, verbose: false);

        [Benchmark]
        public void PerftDefault5() => Backend.Perft.MoveGeneration(Default, 5, verbose: false);
        
        [Benchmark]
        public void PerftDefault6() => Backend.Perft.MoveGeneration(Default, 6, verbose: false);
        
        [Benchmark]
        public void PerftKiwipete1() => Backend.Perft.MoveGeneration(Kiwipete, 1, verbose: false);

        [Benchmark]
        public void PerftKiwipete2() => Backend.Perft.MoveGeneration(Kiwipete, 2, verbose: false);
        
        [Benchmark]
        public void PerftKiwipete3() => Backend.Perft.MoveGeneration(Kiwipete, 3, verbose: false);
        
        [Benchmark]
        public void PerftEpPin1() => Backend.Perft.MoveGeneration(EpPin, 1, verbose: false);

        [Benchmark]
        public void PerftEpPin2() => Backend.Perft.MoveGeneration(EpPin, 2, verbose: false);
        
        [Benchmark]
        public void PerftEpPin3() => Backend.Perft.MoveGeneration(EpPin, 3, verbose: false);
        
        [Benchmark]
        public void PerftEpPin4() => Backend.Perft.MoveGeneration(EpPin, 4, verbose: false);
        
        [Benchmark]
        public void PerftEpPin5() => Backend.Perft.MoveGeneration(EpPin, 5, verbose: false);

    }

}
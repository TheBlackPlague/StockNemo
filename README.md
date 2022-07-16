# StockNemo

<h1 align="center">
	<img
		width="625"
		alt="StockNemo Banner"
		src=".readme/Logo/Banner.png">
</h1>

<h3 align="center">
	C# Chess Engine - Finding Good Moves like Stockfish
</h3>

<p align="center">
	<strong>
		<a href="https://github.com/TheBlackPlague/StockNemo/releases">Releases</a>
        •
		<a href="https://lichess.org/@/StockNemo">Lichess</a>
	</strong>
</p>

<p align="center">
<a href="https://dotnet.microsoft.com/en-us/download">
<img
        alt=".NET"
        src="https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white"
>
</a>
<a href="https://github.com/TheBlackPlague/StockNemo/pkgs/container/stocknemo">
<img
        alt="Docker Image"
        src="https://img.shields.io/badge/docker-%230db7ed.svg?style=for-the-badge&logo=docker&logoColor=white"
>
</a>
</p>

<p align="center">
<a href="https://github.com/TheBlackPlague/StockNemo/actions">
<img
		alt="Build Status"
		src="https://github.com/TheBlackPlague/StockNemo/workflows/.NET Core Build/badge.svg"
>
</a>

<a href="https://github.com/TheBlackPlague/StockNemo/actions">
<img
		alt="Test Status"
		src="https://github.com/TheBlackPlague/StockNemo/workflows/.NET Core Unit Test/badge.svg"
>
</a>
</p>

## Overview
* **Strong and Powerful Analysis**
* **Optimized and High Performant**
* **Compatible with Multiple Platforms & Operating Systems**
* **Free of charge with no hidden fees**
* **Documented Source Code**

## Features
* **Engine:**
  * **Evaluation:**
    * Material Development Evaluation
      * Material Evaluation
      * Piece Development Evaluation
    * Tapered Evaluation
      * Early Game
      * Late Game
  * **Move Policy (Move Ordering)**:
    * Transposition Table Move (Normal Move Generation)
    * Promotion Move
      * Promotion Type
    * Capture Move
      * Most Valuable Victim with Least Valuable Attacker (MVVLVA)
  * **Search:**
    * Cancellation Support
    * Iterative Deepening
      * Depth Data Output
      * Principle Variation Output
    * Aspiration Search
      * Narrow Windows
      * Bound-specific Widening
      * Fallback Threshold
    * Alpha-Beta Negamax
      * Three-fold Repetition Pruning
      * Mate Distance Pruning
      * Material Draw Pruning
        * KvK
        * NKvK and KvNK
        * BKvK and KvBK
      * Transposition Table
        * Exact Cutoff
        * Alpha Unchanged Update
        * Beta Cutoff Update
        * Alpha Beta Delta Cutoff
      * Razoring
        * QSearch Evaluation
        * Shallow Threshold
      * Null Move Pruning
        * Three-ply Depth Reduction
        * Non-Razoring Depth
      * Fail-soft Alpha Beta Pruning
    * Quiescence Search (QSearch)
      * Static Evaluation
        * Beta Cutoff
        * Alpha Update
      * Deep Capture Only Search
      * Fail-soft Alpha Beta Pruning
* **Utility:**
  * PERFT (Speeds upto: **168.3B NPS**)
    * PERFT-Divide Output
    * Smart Multi-threaded Algorithm
      * Single-threaded (Depth < 5)
      * Multi-threaded (Depth >= 5)
        * Depth-First (Depth > 5)
        * Breath-First (Depth = 5)
    * Large In-Memory Transposition Table (50 GB) [SUPPORTED HARDWARE ONLY]

### Much thanks to:
- [Cozy Chess](https://github.com/analog-hors/cozy-chess) by Analog Hors for
providing necessary information and data regarding Fixed-Shift Black Magic
BitBoards.
- [Smallbrain](https://github.com/Disservin/Smallbrain) by Disservin for
providing necessary information and data regarding constant attack tables and
check/pin bitboards. Smallbrain and Disservin have been crucial to the
progress made by StockNemo.
- [Stockfish](https://github.com/official-stockfish/Stockfish) by the
Stockfish Team for providing numerous insights and making a brilliant engine,
used numerous times to debug StockNemo.

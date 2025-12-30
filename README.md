## PCG RNG for .NET

[![License](https://img.shields.io/badge/License-Apache%202.0%20OR%20MIT-blue.svg)](LICENSE-APACHE.txt)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()

This repository contains a .NET port of the PCG (Permuted Congruential Generator) family of random number generators.

The core library lives under `dotnet/src/Pcg` and provides:

- `Pcg32`, `Pcg32OneSeq`, `Pcg32Fast`, `Pcg32Unique`
- `Pcg64`, `Pcg64OneSeq`, `Pcg64Fast`, `Pcg64Unique`
- Arc4-sized 32-bit extended generators:
  - `Pcg32K2`, `Pcg32K64`, `Pcg32K64Oneseq`, `Pcg32K64Fast`
  - `Pcg32C64`, `Pcg32C64Oneseq`, `Pcg32C64Fast`
- `PcgRandom` – a `System.Random`-compatible façade backed by `Pcg32`
- Extension helpers for doubles, integers, bytes, and shuffling

All generators are value types (`struct`) with reproducible, seekable sequences and optional multiple streams.

## Basic usage

Add a project reference to the `Pcg` library (or include it in your solution), then:

```csharp
using Pcg;

// Default pcg32 generator
var rng = new Pcg32();

uint value = rng.Next();          // [0, uint.MaxValue]
uint bounded = rng.Next(100);     // [0, 100)

// Seeded with explicit seed and stream
var seeded = new Pcg32(42UL, 54UL);

// Seek forwards or backwards
seeded.Advance(1_000UL);
seeded.Backstep(500UL);
```

## PcgRandom: System.Random-compatible

`PcgRandom` derives from `System.Random` and can be used anywhere a `Random` is expected:

```csharp
using Pcg;
using System;

Random random = new PcgRandom(seed: 123);

int n = random.Next();           // Non-negative int
int nBounded = random.Next(0, 10); // [0, 10)
double d = random.NextDouble();  // [0.0, 1.0)

var bytes = new byte[16];
random.NextBytes(bytes);
```

You can also access the underlying `Pcg32` if you need advanced features:

```csharp
var prng = new PcgRandom(123);
Pcg32 underlying = prng.UnderlyingGenerator;
```

## Extension helpers

The `PcgExtensions` class adds convenience APIs on top of the `IPcgRng<uint>` / `IPcgRng<ulong>` interface.

```csharp
using Pcg;

var rng = new Pcg32(42UL, 54UL);

// Doubles
double u01 = rng.NextDouble();           // [0.0, 1.0)
double range = rng.NextDouble(-5.0, 5.0); // [-5.0, 5.0)

// Integers
int anyInt = rng.NextInt();              // [0, int.MaxValue]
int small = rng.NextInt(100);            // [0, 100)
int between = rng.NextInt(-10, 10);      // [-10, 10)

// Random bytes
Span<byte> buffer = stackalloc byte[32];
rng.NextBytes(buffer);

// Shuffling
int[] values = { 1, 2, 3, 4, 5, 6, 7, 8 };
rng.Shuffle(values); // Fisher–Yates in-place shuffle
```

The same helpers are also available for 64-bit generators implementing `IPcgRng<ulong>` (for example, `Pcg64`).

## Which generator should I use?

- **General 32-bit default**: `Pcg32` – matches the C++ `pcg32` and is a good all-round choice.
- **Faster 32-bit core**: `Pcg32Fast` – higher throughput, half the period of `Pcg32`.
- **64-bit output**: `Pcg64` / `Pcg64Fast` – analogous to the 32-bit family but with 64-bit results.
- **Arc4-sized 32-bit extended generators**:
  - **General extended default**: `Pcg32K64Fast` – arc4-sized state, k=64, tuned for speed.
  - **Multi-stream C-style**: `Pcg32C64` – C-family variant with 2^63 independent streams.
  - **Single-stream C-style**: `Pcg32C64Oneseq` – C-family, one sequence.
  - **Raw throughput (small state)**: `Pcg32Fast` – if you do not need arc4-sized state, this is usually fastest.

## Testing and verification

The solution includes two test projects under `dotnet/tests`:

- `Pcg.Tests` – property-style and behavioural tests for the generators and helpers
- `Pcg.Oracle.Tests` – JSON-driven "oracle" tests that compare .NET output against the official C++ PCG reference

Run the full test suite from the `dotnet` directory:

```bash
dotnet test
```

## License

This project is dual-licensed under your choice of:

- **Apache License 2.0** ([LICENSE-APACHE.txt](LICENSE-APACHE.txt))
- **MIT License** ([LICENSE-MIT.txt](LICENSE-MIT.txt))

You may use this software under the terms of either license.

## Credits

This is a .NET port of the [PCG Random Number Generator](https://www.pcg-random.org/) created by Melissa O'Neill.

- Original PCG algorithm and C++ implementation: Melissa O'Neill
- PCG website: https://www.pcg-random.org/
- Reference C++ implementation: https://github.com/imneme/pcg-cpp

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

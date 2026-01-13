# PCG Random Number Generator for .NET

A .NET port of the [PCG (Permuted Congruential Generator)](http://www.pcg-random.org/) random number generator family.

## Features

- **Fast**: Comparable performance to the C++ reference implementation
- **High Quality**: Passes rigorous statistical tests (TestU01, PractRand)
- **Small State**: Minimal memory footprint (e.g., 16 bytes for Pcg32)
- **Multiple Streams**: Support for independent parallel streams
- **Seekable**: Can jump forward/backward in the sequence efficiently
- **Reproducible**: Given the same seed, produces identical sequences

## Installation

```bash
dotnet add package Pcg.Random
```

## Supported Platforms

| Target Framework | 32-bit Generators | 64-bit Generators | 128-bit Generators |
|------------------|-------------------|-------------------|---------------------|
| .NET 8.0+        | ✅                | ✅                | ✅                  |
| .NET Standard 2.1| ✅                | ❌                | ❌                  |

> **Note:** 64-bit and 128-bit generators require `UInt128`, which is only available in .NET 7+. On .NET Standard 2.1, only 32-bit generators (`Pcg32*`) are available.

## Quick Start

```csharp
using Pcg;

// Create a generator with default seed
var rng = new Pcg32();

// Generate random numbers
uint value = rng.Next();           // Full range [0, uint.MaxValue]
uint bounded = rng.Next(100);      // Range [0, 100)
uint dice = rng.Next(6) + 1;       // Range [1, 6]

// Seeded generator for reproducibility
var seededRng = new Pcg32(seed: 42, stream: 54);

// Advance/backstep for parallel processing
rng.Advance(1000);    // Skip 1000 values
rng.Backstep(500);    // Go back 500 values
```

## Generator Types

### 32-bit Generators (All Platforms)

| Type | State | Output | Streams | Best For |
|------|-------|--------|---------|----------|
| `Pcg32` | 64-bit | 32-bit | 2^63 | General use (recommended) |
| `Pcg32OneSeq` | 64-bit | 32-bit | 1 | Single-stream applications |
| `Pcg32Fast` | 64-bit | 32-bit | 0 | Maximum speed |
| `Pcg32Unique` | 64-bit | 32-bit | unique | Auto-unique streams |

### 64-bit Generators (.NET 7+ only)

| Type | State | Output | Streams | Best For |
|------|-------|--------|---------|----------|
| `Pcg64` | 128-bit | 64-bit | 2^127 | 64-bit random values |
| `Pcg64OneSeq` | 128-bit | 64-bit | 1 | Single-stream 64-bit |
| `Pcg64Fast` | 128-bit | 64-bit | 0 | Maximum speed 64-bit |
| `Pcg64Unique` | 128-bit | 64-bit | unique | Auto-unique streams |

### Extended Generators (.NET 7+ only)

| Type | Period | Output | Description |
|------|--------|--------|-------------|
| `Pcg64K32` | 2^2176 | 64-bit | Extended generator with huge period |
| `Pcg64C32` | 2^2176 | 64-bit | Extended generator (cheap multiplier) |

## Compatibility

This implementation produces **bit-exact** output matching the C++ reference implementation for the same seeds.

## Building

```bash
cd dotnet
dotnet build
dotnet test
```

## Generating Oracle Test Data

To regenerate oracle test data from the C++ reference:

```bash
cd dotnet/tools/OracleGenerator
cmake -B build
cmake --build build
cd build
./generate_oracle
# Copy oracle_data/*.json to tests/Pcg.Oracle.Tests/OracleData/
```

## License

This project is licensed under the MIT License - see the LICENSE file for details.

The original PCG implementation is dual-licensed under Apache 2.0 and MIT.

## References

- [PCG Random Website](http://www.pcg-random.org/)
- [PCG Paper](http://www.pcg-random.org/paper.html)
- [pcg-cpp Repository](https://github.com/imneme/pcg-cpp)

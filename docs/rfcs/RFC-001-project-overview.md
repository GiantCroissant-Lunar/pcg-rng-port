# RFC-001: PCG Random Number Generator .NET Port - Project Overview

**Status:** Draft  
**Created:** 2024-12-02  
**Author:** Cascade AI

## Summary

This RFC describes the porting of the PCG (Permuted Congruential Generator) random number generator library from C++ to .NET. The PCG family of RNGs provides fast, statistically excellent random numbers with useful features like multiple streams and jump/advance operations.

## Background

### What is PCG?

PCG is a family of random number generators designed by Melissa O'Neill. Key characteristics include:

- **Statistical Quality**: Passes rigorous statistical tests (TestU01, PractRand)
- **Speed**: Very fast generation, competitive with or faster than alternatives
- **Small State**: Minimal memory footprint (e.g., 16 bytes for pcg32)
- **Multiple Streams**: Support for independent parallel streams
- **Seekability**: Can jump forward/backward in the sequence efficiently
- **Reproducibility**: Given the same seed, produces identical sequences

### Source Project Structure

The C++ reference implementation (`pcg-cpp`) consists of:

```
include/
├── pcg_random.hpp     # Main generator implementation (~2000 lines)
├── pcg_extras.hpp     # Support utilities (~670 lines)
└── pcg_uint128.hpp    # 128-bit integer support (~1000 lines)
```

## Generator Family

### Core Generator Types

| Name | State | Output | Stream Type | Use Case |
|------|-------|--------|-------------|----------|
| `pcg32` | 64-bit | 32-bit | Settable | General purpose, recommended |
| `pcg32_oneseq` | 64-bit | 32-bit | Fixed | Single stream applications |
| `pcg32_unique` | 64-bit | 32-bit | Unique per instance | Multiple independent generators |
| `pcg32_fast` | 64-bit | 32-bit | MCG (no stream) | Maximum speed |
| `pcg64` | 128-bit | 64-bit | Settable | 64-bit output needed |
| `pcg64_oneseq` | 128-bit | 64-bit | Fixed | Single stream, 64-bit |
| `pcg64_unique` | 128-bit | 64-bit | Unique per instance | Multiple 64-bit generators |
| `pcg64_fast` | 128-bit | 64-bit | MCG | Maximum speed, 64-bit |

### Output Functions

The PCG family uses several output functions to transform LCG state:

- **XSH RR**: XorShift High, Random Rotate (default for 32-bit output)
- **XSH RS**: XorShift High, Random Shift
- **XSL RR**: XorShift Low, Random Rotate (default for 64-bit output)
- **RXS M XS**: Random XorShift, Multiply, XorShift (highest quality)
- **XSL RR RR**: For 128-bit to 128-bit transformation

### Stream Variants

1. **Settable Stream (`setseq`)**: User can specify stream/sequence ID
2. **One Sequence (`oneseq`)**: Uses a fixed default stream
3. **Unique Stream (`unique`)**: Stream based on memory address
4. **MCG (`mcg`)**: No stream (Multiplicative Congruential Generator)

### Extended Generators

For applications requiring more state, extended generators provide k-dimensional equidistribution:

- `pcg32_k2`, `pcg32_k64`, `pcg32_k1024`, `pcg32_k16384`
- `pcg64_k32`, `pcg64_k1024`
- Cryptographic-style variants: `pcg32_c64`, `pcg32_c1024`, etc.

In this port, the following 32-bit "arc4-sized" extended variants are implemented and tested
bit-for-bit against the C++ reference:

- `pcg32_k2`  → `Pcg32K2` (k=2, setseq)
- `pcg32_k64` → `Pcg32K64` (k=64, setseq)
- `pcg32_k64_oneseq` → `Pcg32K64Oneseq` (k=64, single sequence)
- `pcg32_k64_fast` → `Pcg32K64Fast` (k=64, oneseq, fast XSH-RS base)
- `pcg32_c64` → `Pcg32C64` (C-family, k=64, setseq)
- `pcg32_c64_oneseq` → `Pcg32C64Oneseq` (C-family, k=64, single sequence)
- `pcg32_c64_fast` → `Pcg32C64Fast` (C-family, k=64, fast MCG)

## Goals

### Primary Goals

1. **Correctness**: Produce identical output sequences to the C++ implementation
2. **API Compatibility**: Provide equivalent functionality to C++ API
3. **Performance**: Achieve comparable performance to native C++ implementation
4. **Idiomatic .NET**: Follow .NET conventions and patterns

### Secondary Goals

1. **Comprehensive Coverage**: Port all major generator types
2. **Testing Infrastructure**: Oracle tests comparing against C++ reference
3. **Documentation**: Provide thorough API documentation
4. **NuGet Package**: Publish as a NuGet package

## Non-Goals

1. **Cryptographic Security**: PCG is not intended for cryptographic use
2. **Thread Safety by Default**: Users handle synchronization
3. **Unity/Godot Integration**: May be addressed in future packages

## Success Criteria

1. All oracle tests pass (identical output to C++ for same seeds)
2. Full test coverage for public API
3. Performance within 2x of equivalent C++ code (ideally 1.2x or better)
4. Clean, documented public API

## Related RFCs

- RFC-002: Architecture and Design
- RFC-003: Oracle Testing Strategy
- RFC-004: Implementation Phases

## References

- [PCG Random Website](http://www.pcg-random.org/)
- [PCG Paper](http://www.pcg-random.org/paper.html)
- [pcg-cpp Repository](https://github.com/imneme/pcg-cpp)

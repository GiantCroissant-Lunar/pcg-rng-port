# RFC-002: Architecture and Design

**Status:** Draft  
**Created:** 2024-12-02  
**Author:** Cascade AI

## Summary

This RFC defines the architecture and design patterns for the .NET port of PCG RNG.

## Project Structure

```
dotnet/
├── Pcg.sln
├── src/
│   └── Pcg/
│       ├── Pcg.csproj
│       ├── PcgRandom.cs           # Main public API
│       ├── Generators/
│       │   ├── Pcg32.cs           # pcg32 (setseq_xsh_rr_64_32)
│       │   ├── Pcg32OneSeq.cs     # pcg32_oneseq
│       │   ├── Pcg32Unique.cs     # pcg32_unique
│       │   ├── Pcg32Fast.cs       # pcg32_fast (mcg)
│       │   ├── Pcg64.cs           # pcg64 (setseq_xsl_rr_128_64)
│       │   ├── Pcg64OneSeq.cs     # pcg64_oneseq
│       │   ├── Pcg64Unique.cs     # pcg64_unique
│       │   └── Pcg64Fast.cs       # pcg64_fast (mcg)
│       ├── Internal/
│       │   ├── OutputFunctions.cs  # XSH_RR, XSL_RR, RXS_M_XS, etc.
│       │   ├── StreamTypes.cs      # Stream mixin implementations
│       │   ├── Constants.cs        # Multipliers and increments
│       │   └── UInt128.cs          # 128-bit integer support
│       └── Extensions/
│           └── PcgExtensions.cs    # Shuffle, bounded_rand, etc.
├── tests/
│   ├── Pcg.Tests/
│   │   ├── Pcg.Tests.csproj
│   │   ├── Pcg32Tests.cs
│   │   ├── Pcg64Tests.cs
│   │   └── ...
│   └── Pcg.Oracle.Tests/
│       ├── Pcg.Oracle.Tests.csproj
│       ├── OracleData/            # Pre-generated test vectors from C++
│       │   ├── pcg32.json
│       │   ├── pcg64.json
│       │   └── ...
│       └── OracleTests.cs
└── tools/
    └── OracleGenerator/           # C++ program to generate test vectors
        ├── CMakeLists.txt
        └── generate_oracle.cpp
```

## Type Hierarchy

### Design Decision: Struct vs Class

We will use **struct** for the core generators because:
- PCG generators are small (16-32 bytes typically)
- Value semantics are appropriate (copying a generator copies its state)
- Avoids heap allocation for common use cases
- Matches behavior expectations from C++

### Base Abstractions

```csharp
// Common interface for all PCG generators
public interface IPcgRng<T> where T : unmanaged
{
    T Next();
    T Next(T upperBound);
    void Advance(ulong delta);
    void Backstep(ulong delta);
    
    static abstract T MinValue { get; }
    static abstract T MaxValue { get; }
    static abstract int PeriodPow2 { get; }
}

// For generators that support settable streams
public interface ISettableStream<TState> where TState : unmanaged
{
    TState Stream { get; }
    void SetStream(TState stream);
}
```

### Generator Implementations

```csharp
/// <summary>
/// PCG32 with settable stream. The default and most commonly used generator.
/// State: 64-bit, Output: 32-bit, Streams: 2^63
/// </summary>
public struct Pcg32 : IPcgRng<uint>, ISettableStream<ulong>
{
    private ulong _state;
    private ulong _inc;  // Must be odd
    
    // Default seed matching C++ default
    private const ulong DefaultState = 0xcafef00dd15ea5e5UL;
    private const ulong DefaultStream = 1442695040888963407UL;
    
    // LCG multiplier
    private const ulong Multiplier = 6364136223846793005UL;
    
    public Pcg32(ulong state, ulong stream)
    {
        _inc = (stream << 1) | 1;
        _state = 0;
        _state = Bump(_state) + state;
        _state = Bump(_state);
    }
    
    public Pcg32(ulong state) : this(state, DefaultStream >> 1) { }
    
    public Pcg32() : this(DefaultState, DefaultStream >> 1) { }
    
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(_state);
        return OutputXshRr(oldState);
    }
    
    // ... additional methods
}
```

## 128-bit Integer Support

For `pcg64` generators, we need 128-bit arithmetic. Options:

### Option A: Use System.Int128/UInt128 (.NET 7+)
```csharp
#if NET7_0_OR_GREATER
using UInt128 = System.UInt128;
#else
using UInt128 = Pcg.Internal.UInt128;
#endif
```

### Option B: Custom UInt128 struct (for broader compatibility)
```csharp
[StructLayout(LayoutKind.Sequential)]
public readonly struct UInt128 : IEquatable<UInt128>, IComparable<UInt128>
{
    public readonly ulong Low;
    public readonly ulong High;
    
    // Arithmetic operators
    public static UInt128 operator +(UInt128 a, UInt128 b) { ... }
    public static UInt128 operator *(UInt128 a, UInt128 b) { ... }
    // etc.
}
```

**Decision**: Target .NET 8+ and use `System.UInt128`. Provide a netstandard2.1 build with custom implementation if needed.

## Output Functions

### XSH RR (XorShift High, Random Rotate)

Used by `pcg32` family. Transforms 64-bit state to 32-bit output.

```csharp
internal static class OutputFunctions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XshRr(ulong state)
    {
        // High bits determine rotation amount
        int rot = (int)(state >> 59);
        // XorShift then take high bits
        uint xorshifted = (uint)(((state >> 18) ^ state) >> 27);
        // Rotate right
        return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XslRr(UInt128 state)
    {
        // XorShift Low bits to high, then rotate
        ulong hi = (ulong)(state >> 64);
        ulong lo = (ulong)state;
        ulong xored = hi ^ lo;
        int rot = (int)(hi >> 58);
        return (xored >> rot) | (xored << ((-rot) & 63));
    }
}
```

## Stream Types

### Settable Stream
```csharp
// The increment is stored as: inc_ = (stream << 1) | 1
// This ensures it's always odd (required for full period)
```

### One Sequence
```csharp
// Uses a fixed increment: 1442695040888963407UL (for 64-bit state)
```

### Unique Stream
```csharp
// In C++, uses memory address. In .NET, we can use RuntimeHelpers.GetHashCode
// or require explicit ID assignment
```

### MCG (No Stream)
```csharp
// Increment is 0, but state must always be odd
// Slightly faster (no addition step)
```

## Advance/Backstep Algorithm

The advance operation allows jumping forward (or backward) in the sequence without generating intermediate values. Based on Brown's algorithm:

```csharp
public static ulong Advance(ulong state, ulong delta, ulong mult, ulong inc)
{
    ulong accMult = 1;
    ulong accPlus = 0;
    
    while (delta > 0)
    {
        if ((delta & 1) != 0)
        {
            accMult *= mult;
            accPlus = accPlus * mult + inc;
        }
        inc = (mult + 1) * inc;
        mult *= mult;
        delta >>= 1;
    }
    
    return accMult * state + accPlus;
}
```

## Constants

```csharp
internal static class PcgConstants
{
    // 64-bit state multipliers
    public const ulong Multiplier64 = 6364136223846793005UL;
    public const ulong DefaultIncrement64 = 1442695040888963407UL;
    
    // 32-bit state multipliers (for smaller generators)
    public const uint Multiplier32 = 747796405U;
    public const uint DefaultIncrement32 = 2891336453U;
    
    // 128-bit state multipliers
    public static readonly UInt128 Multiplier128 = new(
        high: 2549297995355413924UL,
        low: 4865540595714422341UL
    );
    
    public static readonly UInt128 DefaultIncrement128 = new(
        high: 6364136223846793005UL,
        low: 1442695040888963407UL
    );
    
    // Cheap multiplier for 128-bit (64-bit only, faster)
    public const ulong CheapMultiplier128 = 0xda942042e4dd58b5UL;
}
```

## API Design

### Primary API

```csharp
// Simple usage
var rng = new Pcg32();
uint value = rng.Next();
uint bounded = rng.Next(100);  // [0, 100)

// Seeded
var rng = new Pcg32(seed: 42, stream: 54);

// Advance/backstep
rng.Advance(1000);  // Skip 1000 values
rng.Backstep(500);  // Go back 500 values

// Extension methods
Span<int> array = stackalloc int[10];
rng.Shuffle(array);
```

### Compatibility with System.Random

```csharp
// Wrapper for System.Random compatibility
public sealed class PcgRandom : Random
{
    private Pcg32 _rng;
    
    public PcgRandom() : this(Pcg32.CreateFromRandomDevice()) { }
    public PcgRandom(int seed) : this(new Pcg32((ulong)seed)) { }
    public PcgRandom(Pcg32 rng) => _rng = rng;
    
    protected override double Sample() => _rng.Next() / (double)uint.MaxValue;
    public override int Next() => (int)(_rng.Next() >> 1);
    public override int Next(int maxValue) => (int)_rng.Next((uint)maxValue);
    // etc.
}
```

## Generator Selection Guidelines

At the architecture level, the library exposes several generator families that map directly to the
PCG reference. The recommended defaults are:

- **32-bit core generators**
  - Use **`Pcg32`** as the general-purpose default (settable stream, 64-bit state, 32-bit output).
  - Use **`Pcg32Fast`** when raw throughput matters more than period (MCG, ~half the period of `Pcg32`).

- **64-bit core generators**
  - Use **`Pcg64`** as the general-purpose 64-bit default.
  - Use **`Pcg64Fast`** for maximum 64-bit throughput.

- **32-bit arc4-sized extended generators (k=64)**
  - **`Pcg32K64Fast`** – recommended "extended" default: arc4-sized state, k=64 equidistribution,
    fast oneseq XSH-RS base (`pcg32_k64_fast`).
  - **`Pcg32C64`** – C-family extended variant with settable stream (`pcg32_c64`), suitable when many
    independent streams are required with arc4-sized state.
  - **`Pcg32C64Oneseq`** – C-family, single-sequence extended variant (`pcg32_c64_oneseq`).
  - **`Pcg32C64Fast`** – C-family fast extended variant (`pcg32_c64_fast`), trading a small amount of
    period for a faster core.

In addition, **`Pcg32K2`** and **`Pcg32K64`** expose the non-C-family extended generators with k=2 and
k=64 respectively, matching `pcg32_k2` and `pcg32_k64` in the reference implementation. For most
applications that explicitly want "arc4-sized" state, `Pcg32K64Fast` or `Pcg32C64` should be preferred.

## Performance Considerations

1. **Inlining**: Use `[MethodImpl(MethodImplOptions.AggressiveInlining)]` for hot paths
2. **Struct layout**: Use `[StructLayout(LayoutKind.Sequential)]` for predictable memory layout
3. **Bit operations**: Use `BitOperations.RotateRight` when available
4. **Bounds checking**: Use `Unsafe` class judiciously for tight loops

## Thread Safety

Generators are **not thread-safe**. For parallel usage:
- Create separate generator instances per thread
- Use locking if shared state is required
- Consider `ThreadLocal<Pcg32>` pattern

## Related RFCs

- RFC-001: Project Overview and Scope
- RFC-003: Oracle Testing Strategy
- RFC-004: Implementation Phases

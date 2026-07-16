# Identical constants do not mean identical streams: portable PCG compatibility

This note records why this library's `Pcg32OneSeq` and the terrain-diffusion
`portable_rng._pcg64_next` (pure-Python PCG XSH-RR 64/32) produce **different**
output streams from the same seed value, despite using the very same LCG constants,
and shows the exact translation that makes them agree bit-for-bit.

A first-pass cross-agent review once assumed compatibility from the matching
constants. The golden-vector tests in
`dotnet/tests/Pcg.Oracle.Tests/Pcg32OneSeqPortableRngCompatTests.cs` pin the truth
so the mistake is not repeated.

## The shared constants

Both implementations use the canonical PCG XSH-RR 64/32 LCG constants:

| name | value |
| --- | --- |
| multiplier | `6364136223846793005` |
| increment  | `1442695040888963407` |

These match `Pcg.Internal.PcgConstants.Multiplier64` / `DefaultIncrement64`,
`portable_rng.PCG64_MULT` / `PCG64_INC`, and the official `pcg-cpp` reference. The
XSH-RR output permutation (`xorshifted = ((state >> 18) ^ state) >> 27`,
`rot = state >> 59`, rotate-right) is also identical. Yet the streams differ,
for two independent reasons.

## Reason 1: the seeding convention

`portable_rng` loads the seed **directly** as the initial LCG state:

```python
state = seed & 0xFFFFFFFFFFFFFFFF      # raw seed, no ritual
state = (state * MULT + INC) & MASK64  # advanced on the first step
```

`Pcg32OneSeq(ulong seed)` applies the canonical PCG **seed ritual** before any
output is produced (`dotnet/src/Pcg/Generators/Pcg32OneSeq.cs`):

```csharp
_state = unchecked(seed + Increment);
_state = Bump(_state);                 // _state = (seed + inc) * mult + inc
```

So for the same `seed`, the two start from different LCG states.

## Reason 2: the advance/output ordering

`portable_rng._pcg64_next` **advances then emits**:

```python
state = (state * MULT + INC) & MASK64  # advance FIRST
x = (((state >> 18) ^ state) >> 27) & 0xFFFFFFFF
rot = state >> 59
out32 = ((x >> rot) | (x << ((32 - rot) & 31))) & 0xFFFFFFFF
return state, out32                    # output from the NEW state
```

`Pcg32OneSeq.Next()` **emits then advances** (`oldState` is the pre-advance state):

```csharp
ulong oldState = _state;
_state = Bump(_state);                 // advance AFTER
return OutputFunctions.XshRr(oldState);
```

Let `LCG^k(s)` denote `k` LCG steps from state `s` (`LCG^0(s) = s`). Then:

- `portable_rng` emits `XshRr(LCG^1(seed))`, `XshRr(LCG^2(seed))`, `XshRr(LCG^3(seed))`, ...
- `Pcg32OneSeq` emits `XshRr(LCG^0(_state))`, `XshRr(LCG^1(_state))`, `XshRr(LCG^2(_state))`, ...

## The translation

To make `Pcg32OneSeq` emit `XshRr(LCG^{i+1}(seed))` on its `i`-th `Next()` call,
install an initial `_state` of `LCG^1(seed)`:

```csharp
ulong seed = 42;
var rng = Pcg32OneSeq.FromRawState(unchecked(seed * 6364136223846793005UL
                                              + 1442695040888963407UL));
// rng.Next() now yields the portable_rng stream bit-for-bit.
```

Equivalently: load `seed * MULT + INC` (one portable advance from the raw seed).
`Pcg32OneSeq.FromRawState(ulong)` bypasses the seed ritual so the caller controls
this alignment directly.

## Why a new member was needed

`Pcg32OneSeq` exposed no way to set the raw LCG state: the only constructor
applies the ritual, and `Advance` / `Backstep` move relative to the current
state. `FromRawState(ulong)` is the single, minimal, documented escape hatch
that installs a state verbatim. It is `public` because cross-runtime stream
compatibility is a real, reusable capability, not a test-only concern.

## Golden vectors (first 8 32-bit outputs per seed)

Generated from the pure-Python `_pcg64_next` path (no numba/numpy required). The
reproduction command is embedded in
`Pcg32OneSeqPortableRngCompatTests.cs`.

| seed | out[0] | out[1] | out[2] | out[3] | out[4] | out[5] | out[6] | out[7] |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `1` | `b99c5774` | `c5344d14` | `91cf3bf5` | `9b84069a` | `2445c23a` | `3dd00434` | `64745ce9` | `05faae15` |
| `42` | `75830bbd` | `0e6dfdb2` | `ce19afdf` | `d8cfe2c3` | `012b061e` | `d6de3682` | `b2439ac1` | `4e0e85d9` |
| `1337` | `466026a8` | `76a68cbc` | `0a029e31` | `3a78cbd4` | `55375c97` | `0f6180d0` | `8ed5b47b` | `1595f361` |
| `0x123456789ABCDEF0` | `7b75f3c1` | `2c1f919a` | `7ec843f4` | `71f83b2e` | `29019ea8` | `57153f69` | `06a8626d` | `8e130446` |
| `0xFFFFFFFFFFFFFFFF` | `ea6f52ec` | `bc82e8d4` | `d57bff22` | `dfd368fa` | `32074c80` | `6d1d9c1d` | `5c1ccac9` | `689fcb07` |

## Out of scope: Marsaglia-polar normals are NOT bit-exact

The portable RNG's `fill_standard_normal` consumes the 32-bit uniform stream
(produced above, and reproducible bit-for-bit) but then routes through the
Marsaglia polar transform, which calls `math.log` and `math.sqrt`. Those libm
transcendentals are not correctly-rounded across runtimes, so the resulting
`float64`/`float32` normal variates are **runtime-dependent** and no bit-exactness
claim is made for them. Only the raw 32-bit uniform stream is guaranteed to match.

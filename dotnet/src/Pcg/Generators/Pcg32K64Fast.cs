using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// Extended 32-bit PCG generator with arc4-sized state, fast oneseq variant.
///
/// <para>
/// Technical details:
/// - Algorithm: pcg32_k64_fast = ext_oneseq_xsh_rs_64_32&lt;6,32,true&gt;
/// - Base: oneseq_xsh_rs_64_32 (MCG-style XorShift High, Random Shift)
/// - State: 64-bit base state + 64-entry 32-bit extension table
/// - Output: 32-bit
/// - Period: 2^2112
/// - Streams: 1 (fixed stream)
/// </para>
///
/// This generator has about as much state as <c>arc4random</c> and is
/// k-dimensionally equidistributed with k = 64, matching the C++
/// <c>pcg32_k64_fast</c> engine bit-for-bit.
/// </summary>
public struct Pcg32K64Fast : IPcgRng<uint>, IEquatable<Pcg32K64Fast>
{
    private Pcg32K64FastEngine _engine;

    public Pcg32K64Fast()
        : this(42UL)
    {
    }

    public Pcg32K64Fast(ulong seed)
    {
        _engine = new Pcg32K64FastEngine(seed);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    // From C++ check-pcg32_k64_fast.out
    public static int PeriodPow2 => 2112;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        return _engine.Next();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next(uint upperBound)
    {
        return _engine.Next(upperBound);
    }

    public void Advance(ulong delta)
    {
        _engine.Advance(delta);
    }

    public void Backstep(ulong delta)
    {
        _engine.Backstep(delta);
    }

    public readonly bool Equals(Pcg32K64Fast other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32K64Fast other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg32K64Fast left, Pcg32K64Fast right) => left.Equals(right);

    public static bool operator !=(Pcg32K64Fast left, Pcg32K64Fast right) => !left.Equals(right);
}

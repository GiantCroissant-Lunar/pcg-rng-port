using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// Extended 32-bit PCG generator with arc4-sized state, C-variant fast MCG.
///
/// <para>
/// Technical details:
/// - Algorithm: pcg32_c64_fast = ext_mcg_xsh_rs_64_32&lt;6,32,false&gt;
/// - Base: mcg_xsh_rs_64_32 (MCG with XorShift High, Random Shift)
/// - State: 64-bit base state + 64-entry 32-bit extension table
/// - Output: 32-bit
/// - Period: 2^2110
/// </para>
///
/// This generator trades a small reduction in period for a slightly faster
/// core while retaining the C-family extended behaviour, matching the C++
/// <c>pcg32_c64_fast</c> engine bit-for-bit.
/// </summary>
public struct Pcg32C64Fast : IPcgRng<uint>, IEquatable<Pcg32C64Fast>
{
    private Pcg32C64FastEngine _engine;

    public Pcg32C64Fast()
        : this(42UL)
    {
    }

    public Pcg32C64Fast(ulong seed)
    {
        _engine = new Pcg32C64FastEngine(seed);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    // From C++ check-pcg32_c64_fast.out
    public static int PeriodPow2 => 2110;

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

    public readonly bool Equals(Pcg32C64Fast other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32C64Fast other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg32C64Fast left, Pcg32C64Fast right) => left.Equals(right);

    public static bool operator !=(Pcg32C64Fast left, Pcg32C64Fast right) => !left.Equals(right);
}

using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// Extended 32-bit PCG generator with arc4-sized state, C-variant with settable stream.
///
/// <para>
/// Technical details:
/// - Algorithm: pcg32_c64 = ext_setseq_xsh_rr_64_32&lt;6,16,false&gt;
/// - Base: setseq_xsh_rr_64_32 (standard Pcg32 core)
/// - State: 64-bit base state + 64-entry 32-bit extension table
/// - Output: 32-bit
/// - Period: 2^2112 (* 2^63 streams)
/// </para>
///
/// This generator has about as much state as <c>arc4random</c>; the C-family
/// variant is intended to be harder to predict (though not for cryptographic
/// use), and matches the C++ <c>pcg32_c64</c> engine bit-for-bit.
/// </summary>
public struct Pcg32C64 : IPcgRng<uint>, IEquatable<Pcg32C64>
{
    private Pcg32C64Engine _engine;

    public Pcg32C64()
        : this(42UL, 54UL)
    {
    }

    public Pcg32C64(ulong seed)
        : this(seed, 54UL)
    {
    }

    public Pcg32C64(ulong seed, ulong stream)
    {
        _engine = new Pcg32C64Engine(seed, stream);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    // From C++ check-pcg32_c64.out
    public static int PeriodPow2 => 2112;

    public static int StreamsPow2 => 63;

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

    public readonly bool Equals(Pcg32C64 other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32C64 other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg32C64 left, Pcg32C64 right) => left.Equals(right);

    public static bool operator !=(Pcg32C64 left, Pcg32C64 right) => !left.Equals(right);
}

using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// Extended 32-bit PCG generator with arc4-sized state, C-variant single sequence.
///
/// <para>
/// Technical details:
/// - Algorithm: pcg32_c64_oneseq = ext_oneseq_xsh_rs_64_32&lt;6,32,false&gt;
/// - Base: oneseq_xsh_rs_64_32 (one-sequence XSH-RS)
/// - State: 64-bit base state + 64-entry 32-bit extension table
/// - Output: 32-bit
/// - Period: 2^2112
/// </para>
///
/// This generator matches the C++ <c>pcg32_c64_oneseq</c> engine bit-for-bit
/// and provides a compact, fixed-stream C-family variant with arc4-sized state.
/// </summary>
public struct Pcg32C64Oneseq : IPcgRng<uint>, IEquatable<Pcg32C64Oneseq>
{
    private Pcg32C64OneseqEngine _engine;

    public Pcg32C64Oneseq()
        : this(42UL)
    {
    }

    public Pcg32C64Oneseq(ulong seed)
    {
        _engine = new Pcg32C64OneseqEngine(seed);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    // From C++ check-pcg32_c64_oneseq.out
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

    public readonly bool Equals(Pcg32C64Oneseq other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32C64Oneseq other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg32C64Oneseq left, Pcg32C64Oneseq right) => left.Equals(right);

    public static bool operator !=(Pcg32C64Oneseq left, Pcg32C64Oneseq right) => !left.Equals(right);
}

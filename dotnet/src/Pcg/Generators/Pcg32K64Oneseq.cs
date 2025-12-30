using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg32K64Oneseq : IPcgRng<uint>, IEquatable<Pcg32K64Oneseq>
{
    private Pcg32K64OneseqEngine _engine;

    public Pcg32K64Oneseq()
        : this(PcgConstants.DefaultState64)
    {
    }

    public Pcg32K64Oneseq(ulong seed)
    {
        _engine = new Pcg32K64OneseqEngine(seed);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    public static int PeriodPow2 => 2110;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next() => _engine.Next();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next(uint upperBound) => _engine.Next(upperBound);

    public void Advance(ulong delta) => _engine.Advance(delta);

    public void Backstep(ulong delta) => _engine.Backstep(delta);

    public readonly bool Equals(Pcg32K64Oneseq other) => _engine.Equals(other._engine);

    public override readonly bool Equals(object? obj) => obj is Pcg32K64Oneseq other && Equals(other);

    public override readonly int GetHashCode() => _engine.GetHashCode();

    public static bool operator ==(Pcg32K64Oneseq left, Pcg32K64Oneseq right) => left.Equals(right);

    public static bool operator !=(Pcg32K64Oneseq left, Pcg32K64Oneseq right) => !left.Equals(right);
}

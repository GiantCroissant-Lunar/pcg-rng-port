using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg64C32Oneseq : IPcgRng<ulong>, IEquatable<Pcg64C32Oneseq>
{
    private Pcg64C32OneseqEngine _engine;

    public Pcg64C32Oneseq()
        : this(42UL)
    {
    }

    public Pcg64C32Oneseq(ulong seed)
    {
        _engine = new Pcg64C32OneseqEngine(seed);
    }

    public static ulong MinValue => 0UL;

    public static ulong MaxValue => ulong.MaxValue;

    public static int PeriodPow2 => 2176;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        return _engine.Next();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next(ulong upperBound)
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

    public readonly bool Equals(Pcg64C32Oneseq other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64C32Oneseq other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg64C32Oneseq left, Pcg64C32Oneseq right) => left.Equals(right);

    public static bool operator !=(Pcg64C32Oneseq left, Pcg64C32Oneseq right) => !left.Equals(right);
}

#if !NETSTANDARD
using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg64C32 : IPcgRng<ulong>, IEquatable<Pcg64C32>
{
    private Pcg64C32Engine _engine;

    public Pcg64C32()
        : this(42UL, 54UL)
    {
    }

    public Pcg64C32(ulong seed)
        : this(seed, 54UL)
    {
    }

    public Pcg64C32(ulong seed, ulong stream)
    {
        _engine = new Pcg64C32Engine(seed, stream);
    }

    public static ulong MinValue => 0UL;

    public static ulong MaxValue => ulong.MaxValue;

    public static int PeriodPow2 => 2176;

    public static int StreamsPow2 => 127;

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

    public void AdvanceBase(UInt128 delta)
    {
        _engine.AdvanceBase(delta);
    }

    public void AdvanceBase(ulong delta)
    {
        _engine.AdvanceBase(delta);
    }

    public readonly bool Equals(Pcg64C32 other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64C32 other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg64C32 left, Pcg64C32 right) => left.Equals(right);

    public static bool operator !=(Pcg64C32 left, Pcg64C32 right) => !left.Equals(right);
}
#endif

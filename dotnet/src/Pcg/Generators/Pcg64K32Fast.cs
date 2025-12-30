using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg64K32Fast : IPcgRng<ulong>, IEquatable<Pcg64K32Fast>
{
    private Pcg64K32FastEngine _engine;

    public Pcg64K32Fast()
        : this(42UL)
    {
    }

    public Pcg64K32Fast(ulong seed)
    {
        _engine = new Pcg64K32FastEngine(seed);
    }

    public static ulong MinValue => 0UL;

    public static ulong MaxValue => ulong.MaxValue;

    public static int PeriodPow2 => 2174;

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

    public readonly bool Equals(Pcg64K32Fast other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64K32Fast other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg64K32Fast left, Pcg64K32Fast right) => left.Equals(right);

    public static bool operator !=(Pcg64K32Fast left, Pcg64K32Fast right) => !left.Equals(right);
}

#if !NETSTANDARD
using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg64K32 : IPcgRng<ulong>, IEquatable<Pcg64K32>
{
    private Pcg64K32Engine _engine;

    public Pcg64K32()
        : this(42UL, 54UL) // default not used in oracles; simple placeholder
    {
    }

    public Pcg64K32(ulong seed)
        : this(seed, 54UL)
    {
    }

    public Pcg64K32(ulong seed, ulong stream)
    {
        _engine = new Pcg64K32Engine(seed, stream);
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

    public readonly bool Equals(Pcg64K32 other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64K32 other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg64K32 left, Pcg64K32 right) => left.Equals(right);

    public static bool operator !=(Pcg64K32 left, Pcg64K32 right) => !left.Equals(right);
}
#endif

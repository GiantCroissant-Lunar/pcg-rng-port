using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg32K2 : IPcgRng<uint>, ISettableStream<ulong>, IEquatable<Pcg32K2>
{
    private Pcg32K2Engine _engine;

    public Pcg32K2()
        : this(PcgConstants.DefaultState64, PcgConstants.DefaultIncrement64 >> 1)
    {
    }

    public Pcg32K2(ulong seed)
        : this(seed, PcgConstants.DefaultIncrement64 >> 1)
    {
    }

    public Pcg32K2(ulong seed, ulong stream)
    {
        _engine = new Pcg32K2Engine(seed, stream);
    }

    public readonly ulong Stream => _engine.Stream;

    public void SetStream(ulong stream)
    {
        _engine.SetStream(stream);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    public static int PeriodPow2 => 128;

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

    public readonly bool Equals(Pcg32K2 other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32K2 other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg32K2 left, Pcg32K2 right) => left.Equals(right);

    public static bool operator !=(Pcg32K2 left, Pcg32K2 right) => !left.Equals(right);
}

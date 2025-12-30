using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg32K64 : IPcgRng<uint>, ISettableStream<ulong>, IEquatable<Pcg32K64>
{
    private Pcg32K64Engine _engine;

    public Pcg32K64()
        : this(PcgConstants.DefaultState64, PcgConstants.DefaultIncrement64 >> 1)
    {
    }

    public Pcg32K64(ulong seed)
        : this(seed, PcgConstants.DefaultIncrement64 >> 1)
    {
    }

    public Pcg32K64(ulong seed, ulong stream)
    {
        _engine = new Pcg32K64Engine(seed, stream);
    }

    public readonly ulong Stream => _engine.Stream;

    public void SetStream(ulong stream)
    {
        _engine.SetStream(stream);
    }

    public static uint MinValue => 0;

    public static uint MaxValue => uint.MaxValue;

    // From C++ check-pcg32_k64.out
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

    public readonly bool Equals(Pcg32K64 other)
    {
        return _engine.Equals(other._engine);
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32K64 other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    public static bool operator ==(Pcg32K64 left, Pcg32K64 right) => left.Equals(right);

    public static bool operator !=(Pcg32K64 left, Pcg32K64 right) => !left.Equals(right);
}

#if !NETSTANDARD
using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg128OnceInsecure : IPcgRng<UInt128>, ISettableStream<UInt128>, IEquatable<Pcg128OnceInsecure>
{
    private UInt128 _state;
    private UInt128 _inc; // Stream value (must be odd)

    public Pcg128OnceInsecure()
        : this(new UInt128(0, PcgConstants.DefaultState64), PcgConstants.DefaultIncrement128 >> 1)
    {
    }

    public Pcg128OnceInsecure(UInt128 seed)
        : this(seed, PcgConstants.DefaultIncrement128 >> 1)
    {
    }

    public Pcg128OnceInsecure(UInt128 seed, UInt128 stream)
    {
        _inc = (stream << 1) | UInt128.One;
        _state = seed + _inc;
        _state = Bump(_state);
    }

    public Pcg128OnceInsecure(ulong seedHi, ulong seedLo, ulong streamHi, ulong streamLo)
        : this(new UInt128(seedHi, seedLo), new UInt128(streamHi, streamLo))
    {
    }

    public UInt128 Stream => _inc >> 1;

    public void SetStream(UInt128 stream)
    {
        _inc = (stream << 1) | UInt128.One;
    }

    public static UInt128 MinValue => UInt128.Zero;

    public static UInt128 MaxValue => new UInt128(ulong.MaxValue, ulong.MaxValue);

    public static int PeriodPow2 => 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128 Next()
    {
        _state = Bump(_state);
        return OutputFunctions.XslRrRr(_state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UInt128 Next(UInt128 upperBound)
    {
        if (upperBound == UInt128.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(upperBound));
        }

        UInt128 threshold = (UInt128.Zero - upperBound) % upperBound;

        UInt128 r;
        do
        {
            r = Next();
        } while (r < threshold);

        return r % upperBound;
    }

    public void Advance(ulong delta)
    {
        Advance(new UInt128(0, delta));
    }

    public void Advance(UInt128 delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier128, _inc);
    }

    public void Backstep(ulong delta)
    {
        Advance(UInt128.Zero - new UInt128(0, delta));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UInt128 Bump(UInt128 state)
    {
        return state * PcgConstants.Multiplier128 + _inc;
    }

    public bool Equals(Pcg128OnceInsecure other)
    {
        return _state == other._state && _inc == other._inc;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pcg128OnceInsecure other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }

    public static bool operator ==(Pcg128OnceInsecure left, Pcg128OnceInsecure right) => left.Equals(right);

    public static bool operator !=(Pcg128OnceInsecure left, Pcg128OnceInsecure right) => !left.Equals(right);
}
#endif

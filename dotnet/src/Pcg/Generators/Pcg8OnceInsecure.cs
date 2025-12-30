using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg8OnceInsecure : IPcgRng<byte>, ISettableStream<byte>, IEquatable<Pcg8OnceInsecure>
{
    private byte _state;
    private byte _inc; // Stream value (must be odd)

    public Pcg8OnceInsecure()
        : this(0, (byte)(PcgConstants.DefaultIncrement8 >> 1))
    {
    }

    public Pcg8OnceInsecure(byte seed)
        : this(seed, (byte)(PcgConstants.DefaultIncrement8 >> 1))
    {
    }

    public Pcg8OnceInsecure(byte seed, byte stream)
    {
        _inc = (byte)((stream << 1) | 1);
        _state = unchecked((byte)(seed + _inc));
        _state = Bump(_state);
    }

    public byte Stream => (byte)(_inc >> 1);

    public void SetStream(byte stream)
    {
        _inc = (byte)((stream << 1) | 1);
    }

    public static byte MinValue => 0;

    public static byte MaxValue => byte.MaxValue;

    public static int PeriodPow2 => 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Next()
    {
        byte oldState = _state;
        _state = Bump(_state);
        return OutputFunctions.RxsMXs8(oldState);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte Next(byte upperBound)
    {
        if (upperBound == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(upperBound));
        }

        uint ub = upperBound;
        uint threshold = (uint)(-(int)ub) % ub;

        uint r;
        do
        {
            r = Next();
        } while (r < threshold);

        return (byte)(r % ub);
    }

    public void Advance(ulong delta)
    {
        ulong s = _state;
        ulong inc = _inc;
        s = PcgMath.Advance(s, delta, PcgConstants.Multiplier8, inc);
        _state = (byte)s;
    }

    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte Bump(byte state)
    {
        return unchecked((byte)(state * PcgConstants.Multiplier8 + _inc));
    }

    public bool Equals(Pcg8OnceInsecure other)
    {
        return _state == other._state && _inc == other._inc;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pcg8OnceInsecure other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }

    public static bool operator ==(Pcg8OnceInsecure left, Pcg8OnceInsecure right) => left.Equals(right);

    public static bool operator !=(Pcg8OnceInsecure left, Pcg8OnceInsecure right) => !left.Equals(right);
}

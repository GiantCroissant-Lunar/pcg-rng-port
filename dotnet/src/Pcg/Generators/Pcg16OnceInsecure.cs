using System;
using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

public struct Pcg16OnceInsecure : IPcgRng<ushort>, ISettableStream<ushort>, IEquatable<Pcg16OnceInsecure>
{
    private ushort _state;
    private ushort _inc; // Stream value (must be odd)

    public Pcg16OnceInsecure()
        : this(0, (ushort)(PcgConstants.DefaultIncrement16 >> 1))
    {
    }

    public Pcg16OnceInsecure(ushort seed)
        : this(seed, (ushort)(PcgConstants.DefaultIncrement16 >> 1))
    {
    }

    public Pcg16OnceInsecure(ushort seed, ushort stream)
    {
        _inc = (ushort)((stream << 1) | 1);
        _state = unchecked((ushort)(seed + _inc));
        _state = Bump(_state);
    }

    public ushort Stream => (ushort)(_inc >> 1);

    public void SetStream(ushort stream)
    {
        _inc = (ushort)((stream << 1) | 1);
    }

    public static ushort MinValue => 0;

    public static ushort MaxValue => ushort.MaxValue;

    public static int PeriodPow2 => 16;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort Next()
    {
        ushort oldState = _state;
        _state = Bump(_state);
        return OutputFunctions.RxsMXs16(oldState);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ushort Next(ushort upperBound)
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

        return (ushort)(r % ub);
    }

    public void Advance(ulong delta)
    {
        ulong s = _state;
        ulong inc = _inc;
        s = PcgMath.Advance(s, delta, PcgConstants.Multiplier16, inc);
        _state = (ushort)s;
    }

    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ushort Bump(ushort state)
    {
        return unchecked((ushort)(state * PcgConstants.Multiplier16 + _inc));
    }

    public bool Equals(Pcg16OnceInsecure other)
    {
        return _state == other._state && _inc == other._inc;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pcg16OnceInsecure other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }

    public static bool operator ==(Pcg16OnceInsecure left, Pcg16OnceInsecure right) => left.Equals(right);

    public static bool operator !=(Pcg16OnceInsecure left, Pcg16OnceInsecure right) => !left.Equals(right);
}

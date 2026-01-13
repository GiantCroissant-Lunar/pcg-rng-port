#if !NETSTANDARD
using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg64C32OneseqEngine : IEquatable<Pcg64C32OneseqEngine>
{
    private UInt128 _state;
    private readonly ulong[] _data;

    private const int TablePow2 = 5;
    private const int TableSize = 1 << TablePow2;
    private const int TableShift = 128 - TablePow2;

    private static readonly UInt128 Increment = PcgConstants.DefaultIncrement128;

    public Pcg64C32OneseqEngine(ulong seed)
    {
        _state = default;
        _data = new ulong[TableSize];

        InitBase(seed);
        SelfInit();
    }

    private void InitBase(ulong seed)
    {
        var seed128 = new UInt128(0, seed);
        _state = seed128 + Increment;
        _state = Bump(_state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Bump(UInt128 state)
    {
        return state * PcgConstants.Multiplier128 + Increment;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextBase()
    {
        _state = Bump(_state);
        return OutputFunctions.XslRr(_state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        ulong rhs = GetExtendedValue();
        ulong lhs = NextBase();
        return lhs ^ rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next(ulong upperBound)
    {
        ulong threshold = (ulong)(-(long)upperBound) % upperBound;

        ulong r;
        do
        {
            r = Next();
        } while (r < threshold);

        return r % upperBound;
    }

    public void Advance(ulong delta)
    {
        var delta128 = new UInt128(0, delta);
        _state = PcgMath.Advance(_state, delta128, PcgConstants.Multiplier128, Increment);
    }

    public void Backstep(ulong delta)
    {
        var delta128 = new UInt128(0, delta);
        var negDelta = UInt128.Zero - delta128;
        _state = PcgMath.Advance(_state, negDelta, PcgConstants.Multiplier128, Increment);
    }

    private void SelfInit()
    {
        ulong lhs = NextBase();
        ulong rhs = NextBase();
        ulong xdiff = lhs - rhs;

        for (int i = 0; i < _data.Length; i++)
        {
            _data[i] = NextBase() ^ xdiff;
        }
    }

    private ulong GetExtendedValue()
    {
        UInt128 state = _state;
        int index = (int)(ulong)(state >> TableShift);
        return _data[index];
    }

    public bool Equals(Pcg64C32OneseqEngine other)
    {
        if (_state != other._state)
            return false;

        if (_data.Length != other._data.Length)
            return false;

        for (int i = 0; i < _data.Length; i++)
        {
            if (_data[i] != other._data[i])
                return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        return obj is Pcg64C32OneseqEngine other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = _state.GetHashCode();
        unchecked
        {
            for (int i = 0; i < _data.Length; i++)
            {
                hash = (hash * 397) ^ (int)_data[i];
            }
        }

        return hash;
    }
}
#endif

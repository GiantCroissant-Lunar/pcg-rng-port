using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg64K32FastEngine : IEquatable<Pcg64K32FastEngine>
{
    private UInt128 _state;
    private readonly ulong[] _data;

    private const int TablePow2 = 5;
    private const int TableSize = 1 << TablePow2;
    private const ulong TableMask = (1UL << TablePow2) - 1UL;

    public Pcg64K32FastEngine(ulong seed)
    {
        _state = default;
        _data = new ulong[TableSize];

        InitBase(seed);
        SelfInit();
    }

    private void InitBase(ulong seed)
    {
        var seed128 = new UInt128(0, seed);
        _state = MakeValidState(seed128);
    }

    private static UInt128 MakeValidState(UInt128 seed)
    {
        // Match Pcg64Fast (mcg_xsl_rr_128_64): low two bits fixed at 3
        return seed | new UInt128(0, 3UL);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Bump(UInt128 state)
    {
        // MCG: no increment, just multiply
        return state * PcgConstants.Multiplier128;
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
        _state = PcgMath.Advance(_state, delta128, PcgConstants.Multiplier128, UInt128.Zero);
    }

    public void Backstep(ulong delta)
    {
        var delta128 = new UInt128(0, delta);
        var negDelta = UInt128.Zero - delta128;
        _state = PcgMath.Advance(_state, negDelta, PcgConstants.Multiplier128, UInt128.Zero);
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
        // For kdd = true and is_mcg = true, drop the low two bits
        UInt128 state = _state >> 2;
        int index = (int)((ulong)state & TableMask);
        return _data[index];
    }

    public bool Equals(Pcg64K32FastEngine other)
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
        return obj is Pcg64K32FastEngine other && Equals(other);
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

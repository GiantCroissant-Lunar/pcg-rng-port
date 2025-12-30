using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg64K32Engine : IEquatable<Pcg64K32Engine>
{
    private UInt128 _state;
    private UInt128 _inc;
    private readonly ulong[] _data; // table of 32 entries

    // Parameters for pcg64_k32 = ext_setseq_xsl_rr_128_64<5,16,true>
    private const int TablePow2 = 5;          // table size = 32
    private const int AdvancePow2 = 16;       // table advance granularity
    private const ulong TableMask = (1UL << TablePow2) - 1UL; // 0..31
    private const ulong TickMask = (1UL << AdvancePow2) - 1UL; // 0xFFFF

    public Pcg64K32Engine(ulong seed, ulong stream)
    {
        _state = default;
        _inc = default;
        _data = new ulong[1 << TablePow2];

        InitBase(seed, stream);
        SelfInit();
    }

    private void InitBase(ulong seed, ulong stream)
    {
        var seed128 = new UInt128(0, seed);
        var stream128 = new UInt128(0, stream);

        _inc = (stream128 << 1) | UInt128.One;
        _state = seed128 + _inc;
        _state = Bump(_state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private UInt128 Bump(UInt128 state)
    {
        return state * PcgConstants.Multiplier128 + _inc;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong NextBase()
    {
        _state = _state * PcgConstants.Multiplier128 + _inc;
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
        AdvanceInternal(delta, forwards: true);
    }

    public void Backstep(ulong delta)
    {
        AdvanceInternal(delta, forwards: false);
    }

    private void AdvanceInternal(ulong distance, bool forwards)
    {
        UInt128 zero = UInt128.Zero;

        if (distance != 0)
        {
            ulong ticks = distance >> AdvancePow2;

            UInt128 fullDist128 = Distance128(_state, zero, PcgConstants.Multiplier128, _inc);
            ulong fullDistToZero = (ulong)fullDist128;

            ulong nextAdvanceDistance = fullDistToZero & TickMask;

            if (!forwards)
            {
                nextAdvanceDistance = unchecked((ulong)(-(long)nextAdvanceDistance)) & TickMask;
            }

            if (nextAdvanceDistance < (distance & TickMask))
            {
                ++ticks;
            }

            if (ticks != 0)
            {
                AdvanceTable(ticks, forwards);
            }
        }

        if (forwards)
        {
            _state = PcgMath.Advance(_state, new UInt128(0, distance), PcgConstants.Multiplier128, _inc);
        }
        else
        {
            var delta128 = new UInt128(0, distance);
            var negDelta = UInt128.Zero - delta128;
            _state = PcgMath.Advance(_state, negDelta, PcgConstants.Multiplier128, _inc);
        }
    }

    private static UInt128 Distance128(UInt128 curState, UInt128 newState, UInt128 mult, UInt128 inc)
    {
        UInt128 bit = UInt128.One;
        UInt128 distance = UInt128.Zero;
        UInt128 curMult = mult;
        UInt128 curInc = inc;

        while (curState != newState)
        {
            if ((curState & bit) != (newState & bit))
            {
                curState = curState * curMult + curInc;
                distance |= bit;
            }

            bit <<= 1;
            curInc = (curMult + UInt128.One) * curInc;
            curMult *= curMult;
        }

        return distance;
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

    private void AdvanceTableSingleTick()
    {
        bool carry = false;

        for (int i = 0; i < _data.Length; i++)
        {
            if (carry)
            {
                carry = InsideOutRxsMXs64.ExternalStep(ref _data[i], (ulong)(i + 1));
            }

            bool carry2 = InsideOutRxsMXs64.ExternalStep(ref _data[i], (ulong)(i + 1));
            carry = carry || carry2;
        }
    }

    private void AdvanceTable(ulong delta, bool forwards)
    {
        UInt128 carry = UInt128.Zero;
        var delta128 = new UInt128(0, delta);

        for (int i = 0; i < _data.Length; i++)
        {
            UInt128 totalDelta = carry + delta128;
            ulong truncDelta = (ulong)totalDelta;
            carry = totalDelta >> 64; // basebits (128) > extbits (64)

            if (InsideOutRxsMXs64.ExternalAdvance(ref _data[i], (ulong)(i + 1), truncDelta, forwards))
            {
                carry += UInt128.One;
            }
        }
    }

    private ulong GetExtendedValue()
    {
        ulong stateLow = (ulong)_state;
        int index = (int)(stateLow & TableMask);

        if ((stateLow & TickMask) == 0UL)
        {
            AdvanceTableSingleTick();
        }

        return _data[index];
    }

    public bool Equals(Pcg64K32Engine other)
    {
        if (_state != other._state || _inc != other._inc)
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
        return obj is Pcg64K32Engine other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = HashCode.Combine(_state, _inc);
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

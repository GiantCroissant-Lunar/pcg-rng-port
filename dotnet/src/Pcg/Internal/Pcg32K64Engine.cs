using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal struct Pcg32K64Engine : IEquatable<Pcg32K64Engine>
{
    private Pcg32Engine _base;
    private readonly uint[] _data; // table of 64 entries

    // Parameters for pcg32_k64 = ext_setseq_xsh_rr_64_32<6,16,true>
    private const int TablePow2 = 6;          // table size = 64
    private const int AdvancePow2 = 16;       // table advance granularity
    private const ulong TableMask = (1UL << TablePow2) - 1UL; // 0..63
    private const ulong TickMask = (1UL << AdvancePow2) - 1UL; // 0xFFFF

    public Pcg32K64Engine(ulong seed, ulong stream)
    {
        _base = new Pcg32Engine(seed, stream);
        _data = new uint[1 << TablePow2];
        SelfInit();
    }

    public ulong Stream => _base.Stream;

    public void SetStream(ulong stream)
    {
        _base.SetStream(stream);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        uint rhs = GetExtendedValue();
        uint lhs = _base.Next();
        return lhs ^ rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next(uint upperBound)
    {
        uint threshold = (uint)(-(int)upperBound) % upperBound;

        uint r;
        do
        {
            r = Next();
        } while (r < threshold);

        return r % upperBound;
    }

    public void Advance(ulong delta)
    {
        Pcg32ExtendedKddHelper.Advance(
            ref _base,
            _data,
            TablePow2,
            AdvancePow2,
            kdd: true,
            distance: delta,
            forwards: true);
    }

    public void Backstep(ulong delta)
    {
        Pcg32ExtendedKddHelper.Advance(
            ref _base,
            _data,
            TablePow2,
            AdvancePow2,
            kdd: true,
            distance: delta,
            forwards: false);
    }

    private void AdvanceInternal(ulong distance, bool forwards)
    {
        const ulong zero = 0UL; // baseclass::is_mcg is false for setseq

        // Tick the table as in extended::advance
        if (distance != 0)
        {
            // ticks = distance >> advance_pow2
            ulong ticks = distance >> AdvancePow2;

            // adv_mask = tick_mask when not MCG
            ulong fullDistToZero = PcgMath.Distance(
                _base.State,
                zero,
                PcgConstants.Multiplier64,
                _base.Increment);

            ulong nextAdvanceDistance = fullDistToZero & TickMask;

            if (!forwards)
            {
                // (-nextAdvanceDistance) & tick_mask
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
            _base.Advance(distance);
        }
        else
        {
            _base.Backstep(distance);
        }
    }

    private void SelfInit()
    {
        // Port of extended::selfinit()
        Pcg32ExtendedKddHelper.SelfInit(ref _base, _data);
    }

    private void AdvanceTableSingleTick()
    {
        bool carry = false;

        for (int i = 0; i < _data.Length; i++)
        {
            if (carry)
            {
                carry = InsideOutRxsMXs32.ExternalStep(ref _data[i], (ulong)(i + 1));
            }

            bool carry2 = InsideOutRxsMXs32.ExternalStep(ref _data[i], (ulong)(i + 1));
            carry = carry || carry2;
        }
    }

    private void AdvanceTable(ulong delta, bool forwards)
    {
        // Port of extended::advance_table(delta, isForwards)
        ulong carry = 0UL;

        for (int i = 0; i < _data.Length; i++)
        {
            ulong totalDelta = carry + delta;
            uint truncDelta = (uint)totalDelta;
            carry = totalDelta >> 32; // basebits (64) > extbits (32)

            if (InsideOutRxsMXs32.ExternalAdvance(ref _data[i], (ulong)(i + 1), truncDelta, forwards))
            {
                carry += 1UL;
            }
        }
    }

    private uint GetExtendedValue()
    {
        return Pcg32ExtendedKddHelper.GetExtendedValue(
            ref _base,
            _data,
            TablePow2,
            AdvancePow2,
            kdd: true);
    }

    public bool Equals(Pcg32K64Engine other)
    {
        if (!_base.Equals(other._base))
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
        return obj is Pcg32K64Engine other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = _base.GetHashCode();
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

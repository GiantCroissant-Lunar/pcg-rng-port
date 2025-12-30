using System;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

internal interface IPcg32ExtendedBase
{
    ulong State { get; set; }
    ulong Increment { get; }
    bool IsMcg { get; }

    uint Next();
    void Advance(ulong delta);
    void Backstep(ulong delta);
}

internal struct Pcg32FastEngineCore : IPcg32ExtendedBase, IEquatable<Pcg32FastEngineCore>
{
    private ulong _state;

    public Pcg32FastEngineCore(ulong seed)
    {
        _state = seed | 3UL;
    }

    public ulong State
    {
        readonly get => _state;
        set => _state = value;
    }

    public ulong Increment => 0UL;

    public bool IsMcg => true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(oldState);
        return OutputFunctions.XshRs(oldState);
    }

    public void Advance(ulong delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier64, 0UL);
    }

    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Bump(ulong state)
    {
        return state * PcgConstants.Multiplier64;
    }

    public readonly bool Equals(Pcg32FastEngineCore other)
    {
        return _state == other._state;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32FastEngineCore other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _state.GetHashCode();
    }
}

internal struct Pcg32OneSeqRsEngineCore : IPcg32ExtendedBase, IEquatable<Pcg32OneSeqRsEngineCore>
{
    private ulong _state;
    private const ulong IncrementConst = PcgConstants.DefaultIncrement64;

    public Pcg32OneSeqRsEngineCore(ulong seed)
    {
        _state = unchecked(seed + IncrementConst);
        _state = Bump(_state);
    }

    public ulong State
    {
        readonly get => _state;
        set => _state = value;
    }

    public ulong Increment => IncrementConst;

    public bool IsMcg => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(oldState);
        return OutputFunctions.XshRs(oldState);
    }

    public void Advance(ulong delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier64, IncrementConst);
    }

    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Bump(ulong state)
    {
        return state * PcgConstants.Multiplier64 + IncrementConst;
    }

    public readonly bool Equals(Pcg32OneSeqRsEngineCore other)
    {
        return _state == other._state;
    }

    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32OneSeqRsEngineCore other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return _state.GetHashCode();
    }
}

internal static class Pcg32ExtendedKddHelper
{
    private const int StateBits = 64;
    private const int TickLimitPow2 = 64;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SelfInit<TBase>(ref TBase @base, uint[] data)
        where TBase : struct, IPcg32ExtendedBase
    {
        uint lhs = @base.Next();
        uint rhs = @base.Next();
        uint xdiff = lhs - rhs;

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = @base.Next() ^ xdiff;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint GetExtendedValue<TBase>(
        ref TBase @base,
        uint[] data,
        int tablePow2,
        int advancePow2,
        bool kdd)
        where TBase : struct, IPcg32ExtendedBase
    {
        ulong state = @base.State;

        if (kdd && @base.IsMcg)
        {
            state >>= 2;
        }

        ulong tableMask = (1UL << tablePow2) - 1UL;
        int tableShift = StateBits - tablePow2;

        bool mayTick = advancePow2 < StateBits && advancePow2 < TickLimitPow2;
        int tickShift = StateBits - advancePow2;
        ulong tickMask = mayTick ? ((1UL << advancePow2) - 1UL) : ~0UL;

        int index = (int)(kdd ? (state & tableMask) : (state >> tableShift));

        if (mayTick)
        {
            bool tick = kdd
                ? ((state & tickMask) == 0UL)
                : ((state >> tickShift) == 0UL);
            if (tick)
            {
                AdvanceTableSingleTick(data);
            }
        }

        return data[index];
    }

    public static void Advance<TBase>(
        ref TBase @base,
        uint[] data,
        int tablePow2,
        int advancePow2,
        bool kdd,
        ulong distance,
        bool forwards)
        where TBase : struct, IPcg32ExtendedBase
    {
        bool mayTick = advancePow2 < StateBits && advancePow2 < TickLimitPow2;

        if (mayTick && distance != 0)
        {
            ulong tickMask = (1UL << advancePow2) - 1UL;
            ulong ticks = distance >> advancePow2;

            ulong nextAdvanceDistance;

            if (@base.IsMcg)
            {
                ulong zero = @base.State & 3UL;
                ulong advMask = tickMask << 2;
                ulong fullDistToZero = Distance(
                    @base.State,
                    zero,
                    PcgConstants.Multiplier64,
                    @base.Increment,
                    advMask);
                nextAdvanceDistance = fullDistToZero;

                if (!forwards)
                {
                    nextAdvanceDistance = unchecked((ulong)(-(long)nextAdvanceDistance)) & tickMask;
                }
            }
            else
            {
                const ulong zero = 0UL;
                ulong fullDistToZero = PcgMath.Distance(
                    @base.State,
                    zero,
                    PcgConstants.Multiplier64,
                    @base.Increment);
                nextAdvanceDistance = fullDistToZero & tickMask;

                if (!forwards)
                {
                    nextAdvanceDistance = unchecked((ulong)(-(long)nextAdvanceDistance)) & tickMask;
                }
            }

            if (nextAdvanceDistance < (distance & tickMask))
            {
                ++ticks;
            }

            if (ticks != 0)
            {
                AdvanceTable(data, ticks, forwards);
            }
        }

        if (forwards)
        {
            @base.Advance(distance);
        }
        else
        {
            @base.Backstep(distance);
        }
    }

    private static void AdvanceTableSingleTick(uint[] data)
    {
        bool carry = false;

        for (int i = 0; i < data.Length; i++)
        {
            if (carry)
            {
                carry = InsideOutRxsMXs32.ExternalStep(ref data[i], (ulong)(i + 1));
            }

            bool carry2 = InsideOutRxsMXs32.ExternalStep(ref data[i], (ulong)(i + 1));
            carry = carry || carry2;
        }
    }

    private static void AdvanceTable(uint[] data, ulong delta, bool forwards)
    {
        ulong carry = 0UL;

        for (int i = 0; i < data.Length; i++)
        {
            ulong totalDelta = carry + delta;
            uint truncDelta = (uint)totalDelta;
            carry = totalDelta >> 32;

            if (InsideOutRxsMXs32.ExternalAdvance(ref data[i], (ulong)(i + 1), truncDelta, forwards))
            {
                carry += 1UL;
            }
        }
    }

    private static ulong Distance(
        ulong curState,
        ulong newState,
        ulong mult,
        ulong inc,
        ulong mask)
    {
        bool isMcg = inc == 0UL;
        ulong theBit = isMcg ? 4UL : 1UL;
        ulong distance = 0UL;
        const ulong ONE = 1UL;

        while ((curState & mask) != (newState & mask))
        {
            if ((curState & theBit) != (newState & theBit))
            {
                curState = curState * mult + inc;
                distance |= theBit;
            }

            theBit <<= 1;
            inc = (mult + ONE) * inc;
            mult *= mult;
        }

        return isMcg ? (distance >> 2) : distance;
    }
}

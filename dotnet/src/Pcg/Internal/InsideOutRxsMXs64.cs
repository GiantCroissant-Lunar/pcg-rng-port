using System;

namespace Pcg.Internal;

internal static class InsideOutRxsMXs64
{
    private const ulong Multiplier64 = PcgConstants.Multiplier64;
    private const ulong Increment64 = PcgConstants.DefaultIncrement64;

    public static bool ExternalStep(ref ulong randval, ulong index)
    {
        ulong state = RxsMXs64Extval.Unoutput(randval);
        ulong inc = unchecked(Increment64 + index * 2UL);

        state = unchecked(state * Multiplier64 + inc);

        ulong result = RxsMXs64Extval.Output(state);
        randval = result;

        ulong zero = 0UL;
        return result == zero;
    }

    public static bool ExternalAdvance(ref ulong randval, ulong index, ulong delta, bool forwards)
    {
        ulong state = RxsMXs64Extval.Unoutput(randval);
        ulong mult = Multiplier64;
        ulong inc = unchecked(Increment64 + index * 2UL);
        ulong zero = 0UL;

        ulong distToZero = PcgMath.Distance(state, zero, mult, inc);

        bool crossesZero;
        if (forwards)
        {
            crossesZero = distToZero <= delta;
        }
        else
        {
            ulong negDist = unchecked(0UL - distToZero);
            crossesZero = negDist <= delta;
            delta = unchecked(0UL - delta);
        }

        state = PcgMath.Advance(state, delta, mult, inc);
        randval = RxsMXs64Extval.Output(state);
        return crossesZero;
    }
}

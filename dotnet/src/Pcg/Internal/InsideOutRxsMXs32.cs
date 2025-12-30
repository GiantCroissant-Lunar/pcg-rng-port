using System;

namespace Pcg.Internal;

internal static class InsideOutRxsMXs32
{
    private const uint Multiplier32 = 747796405U;
    private const uint Increment32 = 2891336453U;

    public static bool ExternalStep(ref uint randval, ulong index)
    {
        uint state = RxsMXs32Extval.Unoutput(randval);
        uint inc = unchecked(Increment32 + (uint)(index * 2UL));

        state = unchecked(state * Multiplier32 + inc);

        uint result = RxsMXs32Extval.Output(state);
        randval = result;

        uint zero = 0U;
        return result == zero;
    }

    public static bool ExternalAdvance(ref uint randval, ulong index, uint delta, bool forwards)
    {
        uint state = RxsMXs32Extval.Unoutput(randval);
        uint mult = Multiplier32;
        uint inc = unchecked(Increment32 + (uint)(index * 2UL));
        uint zero = 0U;

        uint distToZero = Distance(state, zero, mult, inc);

        bool crossesZero;
        if (forwards)
        {
            crossesZero = distToZero <= delta;
        }
        else
        {
            uint negDist = unchecked(0U - distToZero);
            crossesZero = negDist <= delta;
            delta = unchecked(0U - delta);
        }

        state = Advance(state, delta, mult, inc);
        randval = RxsMXs32Extval.Output(state);
        return crossesZero;
    }

    private static uint Advance(uint state, uint delta, uint mult, uint inc)
    {
        uint accMult = 1U;
        uint accPlus = 0U;

        while (delta > 0U)
        {
            if ((delta & 1U) != 0U)
            {
                accMult = unchecked(accMult * mult);
                accPlus = unchecked(accPlus * mult + inc);
            }

            inc = unchecked((mult + 1U) * inc);
            mult = unchecked(mult * mult);
            delta >>= 1;
        }

        return unchecked(accMult * state + accPlus);
    }

    private static uint Distance(uint curState, uint newState, uint mult, uint inc)
    {
        bool isMcg = inc == 0U;
        uint bit = isMcg ? 4U : 1U;
        uint distance = 0U;
        uint curMult = mult;
        uint curInc = inc;

        while (curState != newState)
        {
            if ((curState & bit) != (newState & bit))
            {
                curState = unchecked(curState * curMult + curInc);
                distance |= bit;
            }

            bit <<= 1;
            curInc = unchecked((curMult + 1U) * curInc);
            curMult = unchecked(curMult * curMult);
        }

        return isMcg ? distance >> 2 : distance;
    }
}

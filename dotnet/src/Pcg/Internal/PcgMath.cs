using System.Runtime.CompilerServices;

namespace Pcg.Internal;

/// <summary>
/// Core mathematical operations for PCG generators.
/// </summary>
internal static class PcgMath
{
    /// <summary>
    /// Advances the LCG state by delta steps in O(log(delta)) time.
    /// Based on Brown, "Random Number Generation with Arbitrary Stride",
    /// Transactions of the American Nuclear Society (Nov. 1994).
    /// </summary>
    /// <param name="state">Current LCG state</param>
    /// <param name="delta">Number of steps to advance</param>
    /// <param name="mult">LCG multiplier</param>
    /// <param name="inc">LCG increment</param>
    /// <returns>New state after advancing delta steps</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Advance(ulong state, ulong delta, ulong mult, ulong inc)
    {
        ulong accMult = 1;
        ulong accPlus = 0;

        while (delta > 0)
        {
            if ((delta & 1) != 0)
            {
                accMult *= mult;
                accPlus = accPlus * mult + inc;
            }
            inc = (mult + 1) * inc;
            mult *= mult;
            delta >>= 1;
        }

        return accMult * state + accPlus;
    }

#if !NETSTANDARD
    /// <summary>
    /// Advances the 128-bit LCG state by delta steps.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 Advance(UInt128 state, UInt128 delta, UInt128 mult, UInt128 inc)
    {
        UInt128 accMult = UInt128.One;
        UInt128 accPlus = UInt128.Zero;

        while (delta > UInt128.Zero)
        {
            if ((delta & UInt128.One) != UInt128.Zero)
            {
                accMult *= mult;
                accPlus = accPlus * mult + inc;
            }
            inc = (mult + UInt128.One) * inc;
            mult *= mult;
            delta >>= 1;
        }

        return accMult * state + accPlus;
    }
#endif

    /// <summary>
    /// Calculates the distance between two states (how many steps from cur to target).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Distance(ulong curState, ulong newState, ulong mult, ulong inc)
    {
        bool isMcg = inc == 0;
        ulong bit = isMcg ? 4UL : 1UL;
        ulong distance = 0;
        ulong curMult = mult;
        ulong curInc = inc;

        while (curState != newState)
        {
            if ((curState & bit) != (newState & bit))
            {
                curState = curState * curMult + curInc;
                distance |= bit;
            }

            bit <<= 1;
            curInc = (curMult + 1) * curInc;
            curMult *= curMult;
        }

        return isMcg ? distance >> 2 : distance;
    }

    /// <summary>
    /// Performs bounded random number generation using rejection sampling.
    /// Ensures uniform distribution in [0, upperBound).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint BoundedRand(ref Pcg32 rng, uint upperBound)
    {
        // Calculate threshold for rejection
        // threshold = (2^32 - upperBound) % upperBound
        uint threshold = (uint)(-(int)upperBound) % upperBound;

        // Rejection sampling
        uint r;
        do
        {
            r = rng.Next();
        } while (r < threshold);

        return r % upperBound;
    }

#if !NETSTANDARD
    /// <summary>
    /// Bounded random for 64-bit generators.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong BoundedRand(ref Pcg64 rng, ulong upperBound)
    {
        ulong threshold = (ulong)(-(long)upperBound) % upperBound;

        ulong r;
        do
        {
            r = rng.Next();
        } while (r < threshold);

        return r % upperBound;
    }
#endif
}

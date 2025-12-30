using System;

namespace Pcg.Internal;

internal static class RxsMXs64Extval
{
    private const ulong McgMultiplier64 = 12605985483714917081UL;
    private const ulong McgUnmultiplier64 = 15009553638781119849UL;
    private const int Bits = 64;
    private const int OpBits = 5;
    private const ulong Mask = (1UL << OpBits) - 1UL;

    public static ulong Output(ulong internalState)
    {
        ulong rshift = OpBits != 0
            ? (internalState >> (Bits - OpBits)) & Mask
            : 0UL;

        int totalShift = OpBits + (int)rshift;
        internalState ^= internalState >> totalShift;
        internalState = unchecked(internalState * McgMultiplier64);

        ulong result = internalState;
        result ^= result >> ((2 * Bits + 2) / 3); // 43 for 64-bit
        return result;
    }

    public static ulong Unoutput(ulong value)
    {
        ulong internalState = UnxorshiftRight(value, Bits, (2 * Bits + 2) / 3);
        internalState = unchecked(internalState * McgUnmultiplier64);

        ulong rshift = OpBits != 0
            ? (internalState >> (Bits - OpBits)) & Mask
            : 0UL;

        int totalShift = OpBits + (int)rshift;
        internalState = UnxorshiftRight(internalState, Bits, totalShift);

        return internalState;
    }

    private static ulong UnxorshiftRight(ulong value, int bits, int shift)
    {
        ulong result = value;
        for (int i = shift; i < bits; i <<= 1)
        {
            result ^= result >> i;
        }

        return result;
    }
}

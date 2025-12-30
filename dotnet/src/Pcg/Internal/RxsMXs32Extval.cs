using System;

namespace Pcg.Internal;

internal static class RxsMXs32Extval
{
    private const uint McgMultiplier32 = 277803737U;
    private const uint McgUnmultiplier32 = 2897767785U;
    private const int Bits = 32;
    private const int OpBits = 4;
    private const int Mask = (1 << OpBits) - 1;

    public static uint Output(uint internalState)
    {
        uint rshift = OpBits != 0
            ? (internalState >> (Bits - OpBits)) & Mask
            : 0U;

        int totalShift = OpBits + (int)rshift;
        internalState ^= internalState >> totalShift;
        internalState = unchecked(internalState * McgMultiplier32);

        uint result = internalState;
        result ^= result >> ((2 * Bits + 2) / 3);
        return result;
    }

    public static uint Unoutput(uint value)
    {
        uint internalState = UnxorshiftRight(value, Bits, (2 * Bits + 2) / 3);
        internalState = unchecked(internalState * McgUnmultiplier32);

        uint rshift = OpBits != 0
            ? (internalState >> (Bits - OpBits)) & Mask
            : 0U;

        int totalShift = OpBits + (int)rshift;
        internalState = UnxorshiftRight(internalState, Bits, totalShift);

        return internalState;
    }

    private static uint UnxorshiftRight(uint value, int bits, int shift)
    {
        uint result = value;
        for (int i = shift; i < bits; i <<= 1)
        {
            result ^= result >> i;
        }

        return result;
    }
}

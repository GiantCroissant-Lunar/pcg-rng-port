using System.Numerics;
using System.Runtime.CompilerServices;

namespace Pcg.Internal;

/// <summary>
/// PCG output functions that transform LCG state into random output.
/// These functions provide the "permutation" in Permuted Congruential Generator.
/// </summary>
internal static class OutputFunctions
{
    /// <summary>
    /// XSH RR: XorShift High bits, then Random Rotate.
    /// Used by pcg32 family (64-bit state -> 32-bit output).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XshRr(ulong state)
    {
        // Rotation amount from high 5 bits
        int rot = (int)(state >> 59);
        // XorShift: xor top half into bottom, then take upper 32 bits
        uint xorshifted = (uint)(((state >> 18) ^ state) >> 27);
        // Rotate right by rot
        return BitOperations.RotateRight(xorshifted, rot);
    }

    /// <summary>
    /// XSH RS: XorShift High bits, then Random Shift.
    /// Alternative to XSH RR, slightly faster on some platforms.
    /// Used by pcg32_fast (MCG variant).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint XshRs(ulong state)
    {
        // Calculate shift amount and xorshift parameters
        // For 64->32: opbits=5, mask=31, topspare=5, bottomspare=27
        const int bits = 64;
        const int xtypebits = 32;
        const int sparebits = bits - xtypebits;

        const int opbits =
            (sparebits - 5 >= 64) ? 5 :
            (sparebits - 4 >= 32) ? 4 :
            (sparebits - 3 >= 16) ? 3 :
            (sparebits - 2 >= 4)  ? 2 :
            (sparebits - 1 >= 1)  ? 1 :
            0;

        const int mask = (1 << opbits) - 1;
        const int maxrandshift = mask;
        const int topspare = opbits;
        const int bottomspare = sparebits - topspare;
        const int xshift = topspare + (xtypebits + maxrandshift) / 2;

        int rshift = opbits != 0 ? (int)((state >> (bits - opbits)) & (uint)mask) : 0;
        ulong xored = state ^ (state >> xshift);
        return (uint)(xored >> (bottomspare - maxrandshift + rshift));
    }

    /// <summary>
    /// XSL RR: XorShift Low bits (from high), then Random Rotate.
    /// Used by pcg64 family (128-bit state -> 64-bit output).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong XslRr(UInt128 state)
    {
        ulong hi = (ulong)(state >> 64);
        ulong lo = (ulong)state;
        
        // XOR fold: xor high bits into low bits
        ulong xored = hi ^ lo;
        
        // Rotation amount from high 6 bits of high half
        int rot = (int)(hi >> 58);
        
        return BitOperations.RotateRight(xored, rot);
    }

    /// <summary>
    /// RXS M XS: Random XorShift, Multiply, XorShift.
    /// Most statistically powerful output function.
    /// Used by "once insecure" variants where state = output type.
    /// 32-bit version.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint RxsMXs32(uint state)
    {
        const int bits = 32;
        const int opbits = 4;  // for 32-bit
        const int mask = (1 << opbits) - 1;
        const int rshift = bits - opbits;  // 28
        
        int shift = (int)((state >> rshift) & mask) + ((bits + opbits) / 2);  // shift in range [18, 33]
        state ^= state >> shift;
        state *= PcgConstants.Multiplier32;
        uint result = state ^ (state >> ((2 * bits + 2) / 3));  // xorshift by 22
        return result;
    }

    /// <summary>
    /// RXS M XS: Random XorShift, Multiply, XorShift.
    /// 64-bit version.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong RxsMXs64(ulong state)
    {
        const int bits = 64;
        const int opbits = 5;  // for 64-bit
        const int mask = (1 << opbits) - 1;
        const int rshift = bits - opbits;  // 59
        
        int shift = (int)((state >> rshift) & mask) + ((bits + opbits) / 2);  // shift in range [35, 66]
        state ^= state >> shift;
        state *= PcgConstants.Multiplier64;
        ulong result = state ^ (state >> ((2 * bits + 2) / 3));  // xorshift by 43
        return result;
    }

    /// <summary>
    /// XSL RR RR: Double rotate for 128-bit to 128-bit transformation.
    /// Used by pcg128_once_insecure.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 XslRrRr(UInt128 state)
    {
        ulong hi = (ulong)(state >> 64);
        ulong lo = (ulong)state;
        
        // First rotation on low bits
        int rot1 = (int)(hi >> 58);
        ulong low_rotated = BitOperations.RotateRight(lo ^ hi, rot1);
        
        // Second rotation on high bits
        int rot2 = (int)(lo >> 58);
        ulong hi_rotated = BitOperations.RotateRight(hi, rot2);
        
        return new UInt128(hi_rotated, low_rotated);
    }
}

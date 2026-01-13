namespace Pcg.Internal;

/// <summary>
/// PCG algorithm constants - multipliers and default increments for various state sizes.
/// These constants are derived from the PCG paper and C++ reference implementation.
/// </summary>
internal static class PcgConstants
{
    // ============================================================================
    // 8-bit state constants
    // ============================================================================
    public const byte Multiplier8 = 141;
    public const byte DefaultIncrement8 = 77;

    // ============================================================================
    // 16-bit state constants
    // ============================================================================
    public const ushort Multiplier16 = 12829;
    public const ushort DefaultIncrement16 = 47989;

    // ============================================================================
    // 32-bit state constants
    // ============================================================================
    public const uint Multiplier32 = 747796405U;
    public const uint DefaultIncrement32 = 2891336453U;

    // ============================================================================
    // 64-bit state constants
    // ============================================================================
    public const ulong Multiplier64 = 6364136223846793005UL;
    public const ulong DefaultIncrement64 = 1442695040888963407UL;

    // ============================================================================
    // 128-bit state constants
    // ============================================================================

    /// <summary>
    /// Default multiplier for 128-bit state: 
    /// 2549297995355413924 * 2^64 + 4865540595714422341
    /// </summary>
    public static readonly UInt128 Multiplier128 = new(
        upper: 2549297995355413924UL,
        lower: 4865540595714422341UL
    );

    /// <summary>
    /// Default increment for 128-bit state:
    /// 6364136223846793005 * 2^64 + 1442695040888963407
    /// </summary>
    public static readonly UInt128 DefaultIncrement128 = new(
        upper: 6364136223846793005UL,
        lower: 1442695040888963407UL
    );

    /// <summary>
    /// Cheap (64-bit only) multiplier for 128-bit state.
    /// Provides faster multiplication at cost of slightly reduced quality.
    /// </summary>
    public const ulong CheapMultiplier128 = 0xda942042e4dd58b5UL;

    // ============================================================================
    // Default seeds (matching C++ reference)
    // ============================================================================

    /// <summary>
    /// Default state initialization value: 0xcafef00dd15ea5e5
    /// </summary>
    public const ulong DefaultState64 = 0xcafef00dd15ea5e5UL;
}

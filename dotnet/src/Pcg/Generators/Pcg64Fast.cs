using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// Fast PCG64 random number generator using MCG (Multiplicative Congruential Generator).
/// State: 128-bit, Output: 64-bit, Streams: none (MCG, no increment).
/// </summary>
public struct Pcg64Fast : IPcgRng<ulong>, IEquatable<Pcg64Fast>
{
    private UInt128 _state; // Must remain odd

    /// <summary>
    /// Creates a new Pcg64Fast with default seed.
    /// </summary>
    public Pcg64Fast()
        : this(new UInt128(0, PcgConstants.DefaultState64))
    {
    }

    /// <summary>
    /// Creates a new Pcg64Fast with the specified 64-bit seed (low 64 bits).
    /// </summary>
    public Pcg64Fast(ulong seed)
        : this(new UInt128(0, seed))
    {
    }

    /// <summary>
    /// Creates a new Pcg64Fast with the specified 128-bit seed.
    /// </summary>
    public Pcg64Fast(UInt128 seed)
    {
        // For MCG, state must be odd.
        _state = MakeValidState(seed);
    }

    private static UInt128 MakeValidState(UInt128 seed)
    {
        // For MCG variants, the C++ engine sets the low two bits to 3.
        // This matches state_|=3U behavior in pcg_random.hpp.
        return seed | new UInt128(0, 3UL);
    }

    /// <summary>
    /// Minimum value that can be generated (always 0).
    /// </summary>
    public static ulong MinValue => 0;

    /// <summary>
    /// Maximum value that can be generated (ulong.MaxValue).
    /// </summary>
    public static ulong MaxValue => ulong.MaxValue;

    /// <summary>
    /// Approximate period of this generator as a power of 2.
    /// For 128-bit MCG, the effective period is 2^126.
    /// </summary>
    public static int PeriodPow2 => 126;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        // pcg64_fast (mcg_xsl_rr_128_64) also uses post-advance output.
        _state = Bump(_state);
        return OutputFunctions.XslRr(_state);
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

    /// <summary>
    /// Advances the generator state by the specified number of steps.
    /// </summary>
    public void Advance(ulong delta)
    {
        Advance(new UInt128(0, delta));
    }

    /// <summary>
    /// Advances the generator state by the specified number of steps.
    /// </summary>
    public void Advance(UInt128 delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier128, UInt128.Zero);
    }

    /// <summary>
    /// Steps the generator backward by the specified number of steps.
    /// </summary>
    public void Backstep(ulong delta)
    {
        Advance(UInt128.Zero - new UInt128(0, delta));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Bump(UInt128 state)
    {
        // MCG: no increment, just multiply
        return state * PcgConstants.Multiplier128;
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg64Fast other)
    {
        return _state == other._state;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64Fast other && Equals(other);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return _state.GetHashCode();
    }

    public static bool operator ==(Pcg64Fast left, Pcg64Fast right) => left.Equals(right);
    public static bool operator !=(Pcg64Fast left, Pcg64Fast right) => !left.Equals(right);
}

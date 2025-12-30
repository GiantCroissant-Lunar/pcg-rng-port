using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// Fast PCG32 random number generator using MCG (Multiplicative Congruential Generator).
/// Slightly faster than <see cref="Pcg32"/> but with half the period.
/// 
/// <para>
/// Technical details:
/// - Algorithm: mcg_xsh_rs_64_32 (MCG with XorShift High, Random Shift)
/// - State: 64-bit (must be odd)
/// - Output: 32-bit
/// - Period: 2^62
/// - Streams: None (MCG has no increment)
/// </para>
/// </summary>
public struct Pcg32Fast : IPcgRng<uint>, IEquatable<Pcg32Fast>
{
    private ulong _state;  // Must always be odd

    /// <summary>
    /// Creates a new Pcg32Fast with default seed.
    /// </summary>
    public Pcg32Fast()
        : this(PcgConstants.DefaultState64)
    {
    }

    /// <summary>
    /// Creates a new Pcg32Fast with the specified seed.
    /// </summary>
    /// <param name="seed">Initial seed value</param>
    public Pcg32Fast(ulong seed)
    {
        // MCG state must be odd, so we set the low 2 bits to 3
        // This matches the C++ behavior
        _state = seed | 3;
    }

    /// <summary>
    /// Minimum value that can be generated (always 0).
    /// </summary>
    public static uint MinValue => 0;

    /// <summary>
    /// Maximum value that can be generated (uint.MaxValue).
    /// </summary>
    public static uint MaxValue => uint.MaxValue;

    /// <summary>
    /// The period of this generator as a power of 2.
    /// MCG has period 2^62 (reduced from 2^64 due to no increment).
    /// </summary>
    public static int PeriodPow2 => 62;

    /// <summary>
    /// Generates the next random 32-bit unsigned integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(_state);
        return OutputFunctions.XshRs(oldState);
    }

    /// <summary>
    /// Generates a random 32-bit unsigned integer less than the specified upper bound.
    /// </summary>
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

    /// <summary>
    /// Advances the generator state by the specified number of steps.
    /// </summary>
    public void Advance(ulong delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier64, 0);
    }

    /// <summary>
    /// Steps the generator backward by the specified number of steps.
    /// </summary>
    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong Bump(ulong state)
    {
        // MCG: no increment, just multiply
        return state * PcgConstants.Multiplier64;
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg32Fast other) => _state == other._state;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is Pcg32Fast other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => _state.GetHashCode();

    public static bool operator ==(Pcg32Fast left, Pcg32Fast right) => left.Equals(right);
    public static bool operator !=(Pcg32Fast left, Pcg32Fast right) => !left.Equals(right);
}

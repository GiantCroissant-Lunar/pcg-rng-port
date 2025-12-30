using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// PCG64 random number generator with a fixed single sequence.
/// State: 128-bit, Output: 64-bit, Streams: 1 (fixed).
/// </summary>
public struct Pcg64OneSeq : IPcgRng<ulong>, IEquatable<Pcg64OneSeq>
{
    private UInt128 _state;

    // Fixed increment for one-sequence variant
    private static readonly UInt128 Increment = PcgConstants.DefaultIncrement128;

    /// <summary>
    /// Creates a new Pcg64OneSeq with default seed.
    /// </summary>
    public Pcg64OneSeq()
        : this(new UInt128(0, PcgConstants.DefaultState64))
    {
    }

    /// <summary>
    /// Creates a new Pcg64OneSeq with the specified 64-bit seed (low 64 bits).
    /// </summary>
    public Pcg64OneSeq(ulong seed)
        : this(new UInt128(0, seed))
    {
    }

    /// <summary>
    /// Creates a new Pcg64OneSeq with the specified 128-bit seed.
    /// </summary>
    public Pcg64OneSeq(UInt128 seed)
    {
        // Initialize state: state_ = bump(state + increment())
        _state = seed + Increment;
        _state = Bump(_state);
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
    /// The period of this generator as a power of 2 (128, meaning period is 2^128).
    /// </summary>
    public static int PeriodPow2 => 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        // oneseq_xsl_rr_128_64 also uses post-advance state.
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

    public void Advance(ulong delta)
    {
        Advance(new UInt128(0, delta));
    }

    public void Advance(UInt128 delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier128, Increment);
    }

    public void Backstep(ulong delta)
    {
        Advance(UInt128.Zero - new UInt128(0, delta));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Bump(UInt128 state)
    {
        return state * PcgConstants.Multiplier128 + Increment;
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg64OneSeq other)
    {
        return _state == other._state;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64OneSeq other && Equals(other);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return _state.GetHashCode();
    }

    public static bool operator ==(Pcg64OneSeq left, Pcg64OneSeq right) => left.Equals(right);
    public static bool operator !=(Pcg64OneSeq left, Pcg64OneSeq right) => !left.Equals(right);
}

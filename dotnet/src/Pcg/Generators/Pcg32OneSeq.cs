using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// PCG32 random number generator with a fixed single sequence.
/// Slightly smaller than <see cref="Pcg32"/> since it doesn't store the stream.
/// 
/// <para>
/// Technical details:
/// - Algorithm: oneseq_xsh_rr_64_32
/// - State: 64-bit
/// - Output: 32-bit
/// - Period: 2^64
/// - Streams: 1 (fixed)
/// </para>
/// </summary>
public struct Pcg32OneSeq : IPcgRng<uint>, IEquatable<Pcg32OneSeq>
{
    private ulong _state;

    // Fixed increment for one-sequence variant
    private const ulong Increment = PcgConstants.DefaultIncrement64;

    /// <summary>
    /// Creates a new Pcg32OneSeq with default seed.
    /// </summary>
    public Pcg32OneSeq()
        : this(PcgConstants.DefaultState64)
    {
    }

    /// <summary>
    /// Creates a new Pcg32OneSeq with the specified seed.
    /// </summary>
    /// <param name="seed">Initial seed value</param>
    public Pcg32OneSeq(ulong seed)
    {
        // Initialize state: state_ = bump(state + increment())
        _state = unchecked(seed + Increment);
        _state = Bump(_state);
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
    /// </summary>
    public static int PeriodPow2 => 64;

    /// <summary>
    /// Generates the next random 32-bit unsigned integer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(_state);
        return OutputFunctions.XshRr(oldState);
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
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier64, Increment);
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
        return state * PcgConstants.Multiplier64 + Increment;
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg32OneSeq other) => _state == other._state;

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj) => obj is Pcg32OneSeq other && Equals(other);

    /// <inheritdoc/>
    public override readonly int GetHashCode() => _state.GetHashCode();

    public static bool operator ==(Pcg32OneSeq left, Pcg32OneSeq right) => left.Equals(right);
    public static bool operator !=(Pcg32OneSeq left, Pcg32OneSeq right) => !left.Equals(right);
}

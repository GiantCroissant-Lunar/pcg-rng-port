using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// PCG32 random number generator with settable stream.
/// This is the recommended default generator for most applications.
/// 
/// <para>
/// Technical details:
/// - Algorithm: setseq_xsh_rr_64_32 (XorShift High, Random Rotate)
/// - State: 64-bit
/// - Output: 32-bit
/// - Period: 2^64
/// - Streams: 2^63 independent sequences
/// </para>
/// </summary>
/// <remarks>
/// This generator produces the same output as C++ pcg32 when initialized
/// with the same seed and stream values.
/// </remarks>
public struct Pcg32 : IPcgRng<uint>, ISettableStream<ulong>, IEquatable<Pcg32>
{
    private Pcg32Engine _engine;

    /// <summary>
    /// Creates a new Pcg32 with default seed and stream.
    /// Matches C++ <c>pcg32 rng;</c> default constructor.
    /// </summary>
    public Pcg32()
        : this(PcgConstants.DefaultState64, PcgConstants.DefaultIncrement64 >> 1)
    {
    }

    /// <summary>
    /// Creates a new Pcg32 with the specified seed and default stream.
    /// </summary>
    /// <param name="seed">Initial seed value</param>
    public Pcg32(ulong seed)
        : this(seed, PcgConstants.DefaultIncrement64 >> 1)
    {
    }

    /// <summary>
    /// Creates a new Pcg32 with the specified seed and stream.
    /// </summary>
    /// <param name="seed">Initial seed value</param>
    /// <param name="stream">Stream/sequence selector (2^63 possible streams)</param>
    public Pcg32(ulong seed, ulong stream)
    {
        _engine = new Pcg32Engine(seed, stream);
    }

    /// <summary>
    /// Gets the current stream ID.
    /// </summary>
    public readonly ulong Stream => _engine.Stream;

    /// <summary>
    /// Sets the stream ID. This will change the sequence but not the state.
    /// </summary>
    public void SetStream(ulong stream)
    {
        _engine.SetStream(stream);
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
    /// The period of this generator as a power of 2 (64, meaning period is 2^64).
    /// </summary>
    public static int PeriodPow2 => 64;

    /// <summary>
    /// The number of available streams as a power of 2 (63, meaning 2^63 streams).
    /// </summary>
    public static int StreamsPow2 => 63;

    /// <summary>
    /// Generates the next random 32-bit unsigned integer.
    /// </summary>
    /// <returns>A random value in the range [0, uint.MaxValue]</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        return _engine.Next();
    }

    /// <summary>
    /// Generates a random 32-bit unsigned integer less than the specified upper bound.
    /// Uses rejection sampling to ensure uniform distribution.
    /// </summary>
    /// <param name="upperBound">The exclusive upper bound (must be > 0)</param>
    /// <returns>A random value in the range [0, upperBound)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next(uint upperBound)
    {
        return _engine.Next(upperBound);
    }

    /// <summary>
    /// Advances the generator state by the specified number of steps.
    /// This is useful for parallel processing or skipping values.
    /// </summary>
    /// <param name="delta">Number of steps to advance</param>
    public void Advance(ulong delta)
    {
        _engine.Advance(delta);
    }

    /// <summary>
    /// Steps the generator backward by the specified number of steps.
    /// Equivalent to Advance(-delta) but works with unsigned delta.
    /// </summary>
    /// <param name="delta">Number of steps to go back</param>
    public void Backstep(ulong delta)
    {
        _engine.Backstep(delta);
    }

    /// <summary>
    /// Calculates the distance (number of steps) between this generator
    /// and another generator with the same stream.
    /// </summary>
    /// <param name="other">The other generator state</param>
    /// <returns>Number of steps from this state to the other state</returns>
    public readonly ulong Distance(Pcg32 other)
    {
        return _engine.Distance(other._engine);
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg32 other)
    {
        return _engine.Equals(other._engine);
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32 other && Equals(other);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return _engine.GetHashCode();
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Pcg32 left, Pcg32 right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Pcg32 left, Pcg32 right) => !left.Equals(right);

    /// <summary>
    /// Calculates the distance between two generators.
    /// </summary>
    public static ulong operator -(Pcg32 left, Pcg32 right) => right.Distance(left);
}

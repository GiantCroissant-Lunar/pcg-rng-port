using System.Runtime.CompilerServices;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// PCG64 random number generator with settable stream.
/// For applications requiring 64-bit random values.
/// 
/// <para>
/// Technical details:
/// - Algorithm: setseq_xsl_rr_128_64 (XorShift Low, Random Rotate)
/// - State: 128-bit
/// - Output: 64-bit
/// - Period: 2^128
/// - Streams: 2^127 independent sequences
/// </para>
/// </summary>
/// <remarks>
/// This generator produces the same output as C++ pcg64 when initialized
/// with the same seed and stream values.
/// </remarks>
public struct Pcg64 : IPcgRng<ulong>, ISettableStream<UInt128>, IEquatable<Pcg64>
{
    private UInt128 _state;
    private UInt128 _inc;  // Stream value (must be odd)

    /// <summary>
    /// Creates a new Pcg64 with default seed and stream.
    /// </summary>
    public Pcg64()
        : this(
            new UInt128(0, PcgConstants.DefaultState64),
            PcgConstants.DefaultIncrement128 >> 1)
    {
    }

    /// <summary>
    /// Creates a new Pcg64 with the specified 64-bit seed and default stream.
    /// </summary>
    /// <param name="seed">Initial seed value (low 64 bits)</param>
    public Pcg64(ulong seed)
        : this(new UInt128(0, seed), PcgConstants.DefaultIncrement128 >> 1)
    {
    }

    /// <summary>
    /// Creates a new Pcg64 with the specified 128-bit seed and default stream.
    /// </summary>
    /// <param name="seed">Initial 128-bit seed value</param>
    public Pcg64(UInt128 seed)
        : this(seed, PcgConstants.DefaultIncrement128 >> 1)
    {
    }

    /// <summary>
    /// Creates a new Pcg64 with the specified seed and stream.
    /// </summary>
    /// <param name="seed">Initial 128-bit seed value</param>
    /// <param name="stream">Stream/sequence selector</param>
    public Pcg64(UInt128 seed, UInt128 stream)
    {
        // Ensure increment is odd (matches C++ stream_mixin initialization)
        _inc = (stream << 1) | UInt128.One;

        // Initialize state: state_ = bump(state + increment())
        _state = seed + _inc;
        _state = Bump(_state);
    }

    /// <summary>
    /// Creates a new Pcg64 with the specified seed components and stream components.
    /// </summary>
    /// <param name="seedHi">High 64 bits of seed</param>
    /// <param name="seedLo">Low 64 bits of seed</param>
    /// <param name="streamHi">High 64 bits of stream</param>
    /// <param name="streamLo">Low 64 bits of stream</param>
    public Pcg64(ulong seedHi, ulong seedLo, ulong streamHi, ulong streamLo)
        : this(new UInt128(seedHi, seedLo), new UInt128(streamHi, streamLo))
    {
    }

    /// <summary>
    /// Gets the current stream ID.
    /// </summary>
    public readonly UInt128 Stream => _inc >> 1;

    /// <summary>
    /// Sets the stream ID. This will change the sequence but not the state.
    /// </summary>
    public void SetStream(UInt128 stream)
    {
        _inc = (stream << 1) | UInt128.One;
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

    /// <summary>
    /// The number of available streams as a power of 2 (127, meaning 2^127 streams).
    /// </summary>
    public static int StreamsPow2 => 127;

    /// <summary>
    /// Generates the next random 64-bit unsigned integer.
    /// </summary>
    /// <returns>A random value in the range [0, ulong.MaxValue]</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        // For 128-bit state generators (pcg64), the C++ reference uses
        // the *post-advance* state as input to the output function.
        // That is, operator() returns output(bump(state_)).
        _state = Bump(_state);
        return OutputFunctions.XslRr(_state);
    }

    /// <summary>
    /// Generates a random 64-bit unsigned integer less than the specified upper bound.
    /// Uses rejection sampling to ensure uniform distribution.
    /// </summary>
    /// <param name="upperBound">The exclusive upper bound (must be > 0)</param>
    /// <returns>A random value in the range [0, upperBound)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next(ulong upperBound)
    {
        // Threshold for rejection sampling
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
    /// <param name="delta">Number of steps to advance</param>
    public void Advance(UInt128 delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier128, _inc);
    }

    /// <summary>
    /// Advances the generator state by the specified number of steps (64-bit delta).
    /// </summary>
    /// <param name="delta">Number of steps to advance</param>
    public void Advance(ulong delta)
    {
        Advance(new UInt128(0, delta));
    }

    /// <summary>
    /// Steps the generator backward by the specified number of steps.
    /// </summary>
    /// <param name="delta">Number of steps to go back</param>
    public void Backstep(ulong delta)
    {
        Advance(UInt128.Zero - new UInt128(0, delta));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly UInt128 Bump(UInt128 state)
    {
        return state * PcgConstants.Multiplier128 + _inc;
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg64 other)
    {
        return _state == other._state && _inc == other._inc;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64 other && Equals(other);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Pcg64 left, Pcg64 right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Pcg64 left, Pcg64 right) => !left.Equals(right);
}

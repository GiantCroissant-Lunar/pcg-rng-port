using System.Runtime.CompilerServices;
using System.Threading;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// PCG32 random number generator with a unique stream per instance.
/// State: 64-bit, Output: 32-bit, Streams: up to 2^63 (assigned uniquely).
/// </summary>
public struct Pcg32Unique : IPcgRng<uint>, IEquatable<Pcg32Unique>
{
    private ulong _state;
    private ulong _inc; // Stream value (must be odd)

    private static long _nextStreamId = 0;

    private static ulong GetNextStreamId()
    {
        // Generate a positive, monotonically increasing stream ID
        long id = Interlocked.Increment(ref _nextStreamId);
        return unchecked((ulong)id);
    }

    /// <summary>
    /// Creates a new Pcg32Unique with default seed and a unique stream.
    /// </summary>
    public Pcg32Unique()
        : this(PcgConstants.DefaultState64)
    {
    }

    /// <summary>
    /// Creates a new Pcg32Unique with the specified seed and a unique stream.
    /// </summary>
    /// <param name="seed">Initial seed value.</param>
    public Pcg32Unique(ulong seed)
        : this(seed, GetNextStreamId())
    {
    }

    /// <summary>
    /// Creates a new Pcg32Unique with the specified seed and explicit stream ID.
    /// </summary>
    /// <param name="seed">Initial seed value.</param>
    /// <param name="streamId">Stream/sequence identifier (2^63 possible streams).</param>
    public Pcg32Unique(ulong seed, ulong streamId)
    {
        // Ensure increment is odd (matches C++ stream_mixin initialization)
        _inc = (streamId << 1) | 1;

        // Initialize state: state_ = bump(state + increment())
        _state = unchecked(seed + _inc);
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
    /// The period of this generator as a power of 2 (64, meaning period is 2^64).
    /// </summary>
    public static int PeriodPow2 => 64;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public uint Next()
    {
        ulong oldState = _state;
        _state = Bump(_state);
        return OutputFunctions.XshRr(oldState);
    }

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

    public void Advance(ulong delta)
    {
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier64, _inc);
    }

    public void Backstep(ulong delta)
    {
        Advance(unchecked((ulong)(-(long)delta)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private readonly ulong Bump(ulong state)
    {
        return state * PcgConstants.Multiplier64 + _inc;
    }

    /// <inheritdoc/>
    public readonly bool Equals(Pcg32Unique other)
    {
        return _state == other._state && _inc == other._inc;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg32Unique other && Equals(other);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }

    public static bool operator ==(Pcg32Unique left, Pcg32Unique right) => left.Equals(right);
    public static bool operator !=(Pcg32Unique left, Pcg32Unique right) => !left.Equals(right);
}

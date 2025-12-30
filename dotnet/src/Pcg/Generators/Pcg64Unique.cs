using System.Runtime.CompilerServices;
using System.Threading;
using Pcg.Internal;

namespace Pcg;

/// <summary>
/// PCG64 random number generator with a unique stream per instance.
/// State: 128-bit, Output: 64-bit, Streams: many (unique per instance).
/// </summary>
public struct Pcg64Unique : IPcgRng<ulong>, IEquatable<Pcg64Unique>
{
    private UInt128 _state;
    private UInt128 _inc; // Stream value (must be odd)

    private static long _nextStreamId = 0;

    private static UInt128 GetNextStreamId()
    {
        long id = Interlocked.Increment(ref _nextStreamId);
        return new UInt128(0, unchecked((ulong)id));
    }

    /// <summary>
    /// Creates a new Pcg64Unique with default seed and a unique stream.
    /// </summary>
    public Pcg64Unique()
        : this(new UInt128(0, PcgConstants.DefaultState64))
    {
    }

    /// <summary>
    /// Creates a new Pcg64Unique with the specified 64-bit seed and a unique stream.
    /// </summary>
    public Pcg64Unique(ulong seed)
        : this(new UInt128(0, seed))
    {
    }

    /// <summary>
    /// Creates a new Pcg64Unique with the specified 128-bit seed and a unique stream.
    /// </summary>
    public Pcg64Unique(UInt128 seed)
        : this(seed, GetNextStreamId())
    {
    }

    /// <summary>
    /// Creates a new Pcg64Unique with the specified seed and stream ID.
    /// </summary>
    public Pcg64Unique(UInt128 seed, UInt128 streamId)
    {
        _inc = (streamId << 1) | UInt128.One;
        _state = seed + _inc;
        _state = Bump(_state);
    }

    public static ulong MinValue => 0;
    public static ulong MaxValue => ulong.MaxValue;
    public static int PeriodPow2 => 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ulong Next()
    {
        // unique_xsl_rr_128_64 uses post-advance state for output.
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
        _state = PcgMath.Advance(_state, delta, PcgConstants.Multiplier128, _inc);
    }

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
    public readonly bool Equals(Pcg64Unique other)
    {
        return _state == other._state && _inc == other._inc;
    }

    /// <inheritdoc/>
    public override readonly bool Equals(object? obj)
    {
        return obj is Pcg64Unique other && Equals(other);
    }

    /// <inheritdoc/>
    public override readonly int GetHashCode()
    {
        return HashCode.Combine(_state, _inc);
    }

    public static bool operator ==(Pcg64Unique left, Pcg64Unique right) => left.Equals(right);
    public static bool operator !=(Pcg64Unique left, Pcg64Unique right) => !left.Equals(right);
}

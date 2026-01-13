using System;

namespace Pcg;

/// <summary>
/// Common interface for all PCG random number generators.
/// </summary>
/// <typeparam name="T">Output value type (e.g. uint, ulong).</typeparam>
public interface IPcgRng<T> where T : unmanaged
{
    /// <summary>
    /// Returns the next random value in the full range of <typeparamref name="T"/>.
    /// </summary>
    T Next();

    /// <summary>
    /// Returns a random value in the range [0, <paramref name="upperBound"/>).
    /// </summary>
    /// <param name="upperBound">Exclusive upper bound.</param>
    T Next(T upperBound);

    /// <summary>
    /// Advances the generator state by <paramref name="delta"/> steps.
    /// </summary>
    /// <param name="delta">Number of steps to advance.</param>
    void Advance(ulong delta);

    /// <summary>
    /// Steps the generator backward by <paramref name="delta"/> steps.
    /// </summary>
    /// <param name="delta">Number of steps to go back.</param>
    void Backstep(ulong delta);

#if !NETSTANDARD
    /// <summary>
    /// Minimum value that can be generated.
    /// </summary>
    static abstract T MinValue { get; }

    /// <summary>
    /// Maximum value that can be generated.
    /// </summary>
    static abstract T MaxValue { get; }

    /// <summary>
    /// Period of the generator expressed as a power of two, i.e. period = 2^PeriodPow2.
    /// </summary>
    static abstract int PeriodPow2 { get; }
#endif
}

/// <summary>
/// Interface for generators that support independent streams / sequences.
/// </summary>
/// <typeparam name="TState">Stream identifier type (e.g. ulong, UInt128).</typeparam>
public interface ISettableStream<TState> where TState : unmanaged
{
    /// <summary>
    /// Gets the current stream identifier.
    /// </summary>
    TState Stream { get; }

    /// <summary>
    /// Sets the current stream identifier.
    /// </summary>
    /// <param name="stream">New stream ID.</param>
    void SetStream(TState stream);
}

/// <summary>
/// A <see cref="Random"/> implementation backed by a <see cref="Pcg32"/> generator.
/// </summary>
public sealed class PcgRandom : Random
{
    private Pcg32 _rng;

    /// <summary>
    /// Initializes a new instance of <see cref="PcgRandom"/> using the default <see cref="Pcg32"/> constructor.
    /// </summary>
    public PcgRandom()
        : this(new Pcg32())
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PcgRandom"/> with the specified seed.
    /// </summary>
    /// <param name="seed">The seed value used to initialize the underlying generator.</param>
    public PcgRandom(int seed)
        : this(new Pcg32((ulong)seed))
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="PcgRandom"/> with an existing <see cref="Pcg32"/> instance.
    /// </summary>
    /// <param name="rng">The underlying PCG32 generator.</param>
    public PcgRandom(Pcg32 rng)
    {
        _rng = rng;
    }

    /// <summary>
    /// Gets the underlying <see cref="Pcg32"/> generator used by this instance.
    /// </summary>
    public Pcg32 UnderlyingGenerator => _rng;

    /// <inheritdoc />
    protected override double Sample()
    {
        return _rng.NextDouble();
    }

    /// <inheritdoc />
    public override int Next()
    {
        return _rng.NextInt();
    }

    /// <inheritdoc />
    public override int Next(int maxValue)
    {
        return _rng.NextInt(maxValue);
    }

    /// <inheritdoc />
    public override int Next(int minValue, int maxValue)
    {
        return _rng.NextInt(minValue, maxValue);
    }

    /// <inheritdoc />
    public override double NextDouble()
    {
        return _rng.NextDouble();
    }

    /// <inheritdoc />
    public override void NextBytes(byte[] buffer)
    {
        if (buffer is null) throw new ArgumentNullException(nameof(buffer));
        _rng.NextBytes(buffer.AsSpan());
    }

    /// <inheritdoc />
    public override void NextBytes(Span<byte> buffer)
    {
        _rng.NextBytes(buffer);
    }
}

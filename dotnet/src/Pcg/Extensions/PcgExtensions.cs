using System;
using System.Collections.Generic;

namespace Pcg;

/// <summary>
/// Extension helpers built on top of <see cref="IPcgRng{T}"/>.
/// </summary>
public static class PcgExtensions
{
    /// <summary>
    /// Returns a random double in the range [0, 1) using a 32-bit PCG generator.
    /// </summary>
    public static double NextDouble(this IPcgRng<uint> rng)
    {
        // Divide by 2^32 to obtain a value in [0, 1).
        const double scale = 1.0 / (uint.MaxValue + 1.0);
        return rng.Next() * scale;
    }

    /// <summary>
    /// Returns a random double in the range [0, 1) using a 64-bit PCG generator.
    /// </summary>
    public static double NextDouble(this IPcgRng<ulong> rng)
    {
        // Divide by 2^64 to obtain a value in [0, 1).
        const double scale = 1.0 / 18446744073709551616.0; // 2^64
        return rng.Next() * scale;
    }

    /// <summary>
    /// Returns a random double in the range [minValue, maxValue).
    /// </summary>
    public static double NextDouble(this IPcgRng<uint> rng, double minValue, double maxValue)
    {
        if (maxValue <= minValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

        return minValue + (maxValue - minValue) * rng.NextDouble();
    }

    /// <summary>
    /// Returns a random double in the range [minValue, maxValue).
    /// </summary>
    public static double NextDouble(this IPcgRng<ulong> rng, double minValue, double maxValue)
    {
        if (maxValue <= minValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

        return minValue + (maxValue - minValue) * rng.NextDouble();
    }

    /// <summary>
    /// Shuffles the contents of the given span in-place using the Fisher-Yates algorithm.
    /// </summary>
    public static void Shuffle<T>(this IPcgRng<uint> rng, Span<T> span)
    {
        for (int i = span.Length - 1; i > 0; i--)
        {
            uint j = rng.Next((uint)(i + 1));
            (span[i], span[(int)j]) = (span[(int)j], span[i]);
        }
    }

    /// <summary>
    /// Shuffles the contents of the given span in-place using the Fisher-Yates algorithm.
    /// </summary>
    public static void Shuffle<T>(this IPcgRng<ulong> rng, Span<T> span)
    {
        for (int i = span.Length - 1; i > 0; i--)
        {
            ulong j = rng.Next((ulong)(i + 1));
            (span[i], span[(int)j]) = (span[(int)j], span[i]);
        }
    }

    /// <summary>
    /// Shuffles the contents of the given list in-place using the Fisher-Yates algorithm.
    /// </summary>
    public static void Shuffle<T>(this IPcgRng<uint> rng, IList<T> list)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));

        for (int i = list.Count - 1; i > 0; i--)
        {
            uint j = rng.Next((uint)(i + 1));
            (list[i], list[(int)j]) = (list[(int)j], list[i]);
        }
    }

    /// <summary>
    /// Shuffles the contents of the given list in-place using the Fisher-Yates algorithm.
    /// </summary>
    public static void Shuffle<T>(this IPcgRng<ulong> rng, IList<T> list)
    {
        if (list is null) throw new ArgumentNullException(nameof(list));

        for (int i = list.Count - 1; i > 0; i--)
        {
            ulong j = rng.Next((ulong)(i + 1));
            (list[i], list[(int)j]) = (list[(int)j], list[i]);
        }
    }

    /// <summary>
    /// Returns a non-negative 32-bit integer in the range [0, int.MaxValue].
    /// </summary>
    public static int NextInt(this IPcgRng<uint> rng)
    {
        // Drop the sign bit to match typical Random behaviour.
        return (int)(rng.Next() >> 1);
    }

    /// <summary>
    /// Returns a non-negative 32-bit integer less than the specified maximum.
    /// </summary>
    public static int NextInt(this IPcgRng<uint> rng, int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be positive.");

        return (int)rng.Next((uint)maxValue);
    }

    /// <summary>
    /// Returns a 32-bit integer in the range [minValue, maxValue).
    /// </summary>
    public static int NextInt(this IPcgRng<uint> rng, int minValue, int maxValue)
    {
        if (maxValue <= minValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

        uint range = (uint)((long)maxValue - minValue);
        return minValue + (int)rng.Next(range);
    }

    /// <summary>
    /// Returns a non-negative 32-bit integer using a 64-bit generator.
    /// </summary>
    public static int NextInt(this IPcgRng<ulong> rng)
    {
        // Use bounded Next for unbiased 31-bit values.
        return (int)rng.Next(1UL << 31);
    }

    /// <summary>
    /// Returns a non-negative 32-bit integer less than the specified maximum using a 64-bit generator.
    /// </summary>
    public static int NextInt(this IPcgRng<ulong> rng, int maxValue)
    {
        if (maxValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be positive.");

        return (int)rng.Next((ulong)maxValue);
    }

    /// <summary>
    /// Returns a 32-bit integer in the range [minValue, maxValue) using a 64-bit generator.
    /// </summary>
    public static int NextInt(this IPcgRng<ulong> rng, int minValue, int maxValue)
    {
        if (maxValue <= minValue)
            throw new ArgumentOutOfRangeException(nameof(maxValue), "maxValue must be greater than minValue.");

        ulong range = (ulong)((long)maxValue - minValue);
        return minValue + (int)rng.Next(range);
    }

    /// <summary>
    /// Fills the specified buffer with random bytes using a 32-bit generator.
    /// </summary>
    public static void NextBytes(this IPcgRng<uint> rng, Span<byte> buffer)
    {
        int index = 0;
        while (index < buffer.Length)
        {
            uint value = rng.Next();
            for (int i = 0; i < 4 && index < buffer.Length; i++, index++)
            {
                buffer[index] = (byte)(value & 0xFF);
                value >>= 8;
            }
        }
    }

    /// <summary>
    /// Fills the specified buffer with random bytes using a 64-bit generator.
    /// </summary>
    public static void NextBytes(this IPcgRng<ulong> rng, Span<byte> buffer)
    {
        int index = 0;
        while (index < buffer.Length)
        {
            ulong value = rng.Next();
            for (int i = 0; i < 8 && index < buffer.Length; i++, index++)
            {
                buffer[index] = (byte)(value & 0xFF);
                value >>= 8;
            }
        }
    }
}

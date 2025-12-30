using System;
using System.Collections.Generic;
using Xunit;

namespace Pcg.Tests;

public class PcgExtensionsTests
{
    [Fact]
    public void NextDouble32_ReturnsValuesWithinRange()
    {
        IPcgRng<uint> rng = new Pcg32(42, 54);

        for (int i = 0; i < 1000; i++)
        {
            double value = rng.NextDouble();
            Assert.InRange(value, 0.0, 1.0);
            Assert.True(value < 1.0);
        }
    }

    [Fact]
    public void NextDouble64_ReturnsValuesWithinRange()
    {
        IPcgRng<ulong> rng = new Pcg64(42, 54);

        for (int i = 0; i < 1000; i++)
        {
            double value = rng.NextDouble();
            Assert.InRange(value, 0.0, 1.0);
            Assert.True(value < 1.0);
        }
    }

    [Fact]
    public void NextDouble_WithRange_ProducesValuesInInterval()
    {
        IPcgRng<uint> rng = new Pcg32(42, 54);

        const double min = -5.0;
        const double max = 10.0;

        for (int i = 0; i < 1000; i++)
        {
            double value = rng.NextDouble(min, max);
            Assert.InRange(value, min, max);
            Assert.True(value < max);
        }
    }

    [Fact]
    public void ShuffleSpan32_ReordersElementsButPreservesSet()
    {
        IPcgRng<uint> rng = new Pcg32(42, 54);

        int[] original = { 1, 2, 3, 4, 5, 6, 7, 8 };
        int[] shuffled = (int[])original.Clone();

        rng.Shuffle(shuffled.AsSpan());

        Array.Sort(original);
        Array.Sort(shuffled);

        Assert.Equal(original, shuffled);
    }

    [Fact]
    public void ShuffleList64_ReordersElementsButPreservesSet()
    {
        IPcgRng<ulong> rng = new Pcg64(42, 54);

        var original = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var shuffled = new List<int>(original);

        rng.Shuffle(shuffled);

        original.Sort();
        shuffled.Sort();

        Assert.Equal(original, shuffled);
    }

    [Fact]
    public void NextInt32_WithBounds_WithinRange()
    {
        IPcgRng<uint> rng = new Pcg32(42, 54);

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.NextInt(100);
            Assert.InRange(value, 0, 99);
        }
    }

    [Fact]
    public void NextInt32_WithMinMax_WithinRange()
    {
        IPcgRng<uint> rng = new Pcg32(42, 54);

        const int min = -10;
        const int max = 10;

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.NextInt(min, max);
            Assert.InRange(value, min, max);
            Assert.True(value < max);
        }
    }

    [Fact]
    public void NextInt64_WithMinMax_WithinRange()
    {
        IPcgRng<ulong> rng = new Pcg64(42, 54);

        const int min = -100;
        const int max = 100;

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.NextInt(min, max);
            Assert.InRange(value, min, max);
            Assert.True(value < max);
        }
    }

    [Fact]
    public void NextBytes32_IsDeterministicForGivenSeed()
    {
        var rng1 = new Pcg32(42, 54);
        var rng2 = new Pcg32(42, 54);

        byte[] buffer1 = new byte[32];
        byte[] buffer2 = new byte[32];

        rng1.NextBytes(buffer1);
        rng2.NextBytes(buffer2);

        Assert.Equal(buffer1, buffer2);
    }

    [Fact]
    public void NextBytes64_IsDeterministicForGivenSeed()
    {
        var rng1 = new Pcg64(42, 54);
        var rng2 = new Pcg64(42, 54);

        byte[] buffer1 = new byte[32];
        byte[] buffer2 = new byte[32];

        rng1.NextBytes(buffer1);
        rng2.NextBytes(buffer2);

        Assert.Equal(buffer1, buffer2);
    }
}

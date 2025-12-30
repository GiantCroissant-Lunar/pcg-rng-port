using System;
using Xunit;

namespace Pcg.Tests;

public class PcgRandomTests
{
    [Fact]
    public void SeededInstances_ProduceSameSequence()
    {
        var rng1 = new PcgRandom(123);
        var rng2 = new PcgRandom(123);

        for (int i = 0; i < 1000; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Next_WithBounds_WithinRange()
    {
        var rng = new PcgRandom(42);

        for (int i = 0; i < 1000; i++)
        {
            int value = rng.Next(1, 100);
            Assert.InRange(value, 1, 99);
        }
    }

    [Fact]
    public void NextDouble_WithinRange()
    {
        var rng = new PcgRandom(42);

        for (int i = 0; i < 1000; i++)
        {
            double value = rng.NextDouble();
            Assert.InRange(value, 0.0, 1.0);
            Assert.True(value < 1.0);
        }
    }

    [Fact]
    public void NextBytes_IsDeterministicForSeed()
    {
        var rng1 = new PcgRandom(123);
        var rng2 = new PcgRandom(123);

        byte[] buffer1 = new byte[64];
        byte[] buffer2 = new byte[64];

        rng1.NextBytes(buffer1);
        rng2.NextBytes(buffer2);

        Assert.Equal(buffer1, buffer2);
    }
}

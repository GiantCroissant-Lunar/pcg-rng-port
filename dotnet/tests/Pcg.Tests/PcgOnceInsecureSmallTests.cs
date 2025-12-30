using Xunit;

namespace Pcg.Tests;

public class PcgOnceInsecureSmallTests
{
    [Fact]
    public void Pcg8_Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg8OnceInsecure(42, 7);
        var rng2 = new Pcg8OnceInsecure(42, 7);

        for (int i = 0; i < 1000; i++)
        {
            rng1.Next();
        }

        rng2.Advance(1000);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Pcg8_Backstep_RewindsToPreviousValues()
    {
        var rng = new Pcg8OnceInsecure(42, 7);

        byte[] values = new byte[100];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = rng.Next();
        }

        rng.Backstep((ulong)values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], rng.Next());
        }
    }

    [Fact]
    public void Pcg16_Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg16OnceInsecure(42, 7);
        var rng2 = new Pcg16OnceInsecure(42, 7);

        for (int i = 0; i < 1000; i++)
        {
            rng1.Next();
        }

        rng2.Advance(1000);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Pcg16_Backstep_RewindsToPreviousValues()
    {
        var rng = new Pcg16OnceInsecure(42, 7);

        ushort[] values = new ushort[100];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = rng.Next();
        }

        rng.Backstep((ulong)values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], rng.Next());
        }
    }

    [Fact]
    public void Pcg128_Advance_MatchesSequentialGeneration()
    {
        var seed = new UInt128(0, 42UL);
        var stream = new UInt128(0, 54UL);

        var rng1 = new Pcg128OnceInsecure(seed, stream);
        var rng2 = new Pcg128OnceInsecure(seed, stream);

        for (int i = 0; i < 1000; i++)
        {
            rng1.Next();
        }

        rng2.Advance(1000UL);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }

    [Fact]
    public void Pcg128_Backstep_RewindsToPreviousValues()
    {
        var seed = new UInt128(0, 42UL);
        var stream = new UInt128(0, 54UL);

        var rng = new Pcg128OnceInsecure(seed, stream);

        UInt128[] values = new UInt128[100];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = rng.Next();
        }

        rng.Backstep((ulong)values.Length);

        for (int i = 0; i < values.Length; i++)
        {
            Assert.Equal(values[i], rng.Next());
        }
    }
}

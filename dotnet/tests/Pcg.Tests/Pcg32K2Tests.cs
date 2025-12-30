using Xunit;

namespace Pcg.Tests;

public class Pcg32K2Tests
{
    [Fact]
    public void Advance_MatchesSequentialGeneration()
    {
        var rng1 = new Pcg32K2(42, 54);
        var rng2 = new Pcg32K2(42, 54);

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
    public void Backstep_RewindsToPreviousValues()
    {
        var rng = new Pcg32K2(42, 54);

        uint[] values = new uint[100];
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

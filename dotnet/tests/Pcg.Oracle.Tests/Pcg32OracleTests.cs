using Xunit;

namespace Pcg.Oracle.Tests;

/// <summary>
/// Oracle tests that compare Pcg32 output against the C++ reference implementation.
/// </summary>
public class Pcg32OracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg32");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg32_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0;
        ulong stream = testCase.Stream != null ? ulong.Parse(testCase.Stream) : 0;

        var rng = new Pcg32(seed, stream);

        foreach (var entry in testCase.Sequence)
        {
            uint actual = rng.Next();
            uint expected = entry.AsUInt32();
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Pcg32_Seed42Stream54_MatchesKnownValues()
    {
        // Known values from C++ reference: pcg32(42u, 54u)
        // These are the first 6 values from check-pcg32.out
        var rng = new Pcg32(42, 54);

        Assert.Equal(0xa15c02b7u, rng.Next());
        Assert.Equal(0x7b47f409u, rng.Next());
        Assert.Equal(0xba1d3330u, rng.Next());
        Assert.Equal(0x83d2f293u, rng.Next());
        Assert.Equal(0xbfa4784bu, rng.Next());
        Assert.Equal(0xcbed606eu, rng.Next());
    }

    [Fact]
    public void Pcg32_Backstep_MatchesKnownValues()
    {
        // Test the backstep functionality matches C++ output
        // From check-pcg32.out: Generate 6 values, backstep 6, should get same values
        var rng = new Pcg32(42, 54);

        // Generate first 6
        uint[] first = new uint[6];
        for (int i = 0; i < 6; i++)
        {
            first[i] = rng.Next();
        }

        // Backstep
        rng.Backstep(6);

        // Should get same values
        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(first[i], rng.Next());
        }
    }

    [Fact]
    public void Pcg32_BoundedRandom_StaysWithinBounds()
    {
        var rng = new Pcg32(42, 54);

        // Test various bounds
        for (int i = 0; i < 1000; i++)
        {
            uint val = rng.Next(100);
            Assert.True(val < 100);
        }

        for (int i = 0; i < 1000; i++)
        {
            uint val = rng.Next(6);
            Assert.True(val < 6);
        }
    }

    [Fact]
    public void Pcg32_Advance_SkipsCorrectly()
    {
        // Test that Advance skips the correct number of values
        var rng1 = new Pcg32(42, 54);
        var rng2 = new Pcg32(42, 54);
        
        // Generate 100 values with rng1
        for (int i = 0; i < 100; i++)
        {
            rng1.Next();
        }
        
        // Skip 100 values with rng2
        rng2.Advance(100);
        
        // They should now produce the same next 10 values
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(rng1.Next(), rng2.Next());
        }
    }
}

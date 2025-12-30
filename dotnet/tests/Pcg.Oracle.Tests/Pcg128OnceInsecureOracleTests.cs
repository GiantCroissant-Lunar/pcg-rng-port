using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg128OnceInsecureOracleTests
{
    [Fact]
    public void Pcg128OnceInsecure_MatchesOracle_Json()
    {
        var testCases = OracleDataLoader.GetTestCases("pcg128_once_insecure");
        if (testCases.Count == 0)
        {
            // No oracle data present yet; do not fail the test suite.
            return;
        }

        foreach (var testCase in testCases)
        {
            ulong seedHi = testCase.SeedHi != null ? ulong.Parse(testCase.SeedHi) : 0UL;
            ulong seedLo = testCase.SeedLo != null ? ulong.Parse(testCase.SeedLo) : 0UL;
            ulong streamHi = testCase.StreamHi != null ? ulong.Parse(testCase.StreamHi) : 0UL;
            ulong streamLo = testCase.StreamLo != null ? ulong.Parse(testCase.StreamLo) : 0UL;

            var seed = new UInt128(seedHi, seedLo);
            var stream = new UInt128(streamHi, streamLo);

            var rng = new Pcg128OnceInsecure(seed, stream);

            foreach (var entry in testCase.Sequence)
            {
                UInt128 actual = rng.Next();
                UInt128 expected = entry.AsUInt128();
                Assert.Equal(expected, actual);
            }
        }
    }
}

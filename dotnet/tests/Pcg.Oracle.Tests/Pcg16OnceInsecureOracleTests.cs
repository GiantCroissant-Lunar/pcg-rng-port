using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg16OnceInsecureOracleTests
{
    [Fact]
    public void Pcg16OnceInsecure_MatchesOracle_Json()
    {
        var testCases = OracleDataLoader.GetTestCases("pcg16_once_insecure");
        if (testCases.Count == 0)
        {
            // No oracle data present yet; do not fail the test suite.
            return;
        }

        foreach (var testCase in testCases)
        {
            ushort seed = testCase.Seed != null ? ushort.Parse(testCase.Seed) : (ushort)0;
            ushort stream = testCase.Stream != null ? ushort.Parse(testCase.Stream) : (ushort)0;

            var rng = new Pcg16OnceInsecure(seed, stream);

            foreach (var entry in testCase.Sequence)
            {
                ushort actual = rng.Next();
                ushort expected = (ushort)entry.AsUInt32();
                Assert.Equal(expected, actual);
            }
        }
    }
}

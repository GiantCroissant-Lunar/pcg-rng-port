using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32C64FastOracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg32_c64_fast");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg32C64Fast_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0UL;

        var rng = new Pcg32C64Fast(seed);

        foreach (var entry in testCase.Sequence)
        {
            uint actual = rng.Next();
            uint expected = entry.AsUInt32();
            Assert.Equal(expected, actual);
        }
    }
}

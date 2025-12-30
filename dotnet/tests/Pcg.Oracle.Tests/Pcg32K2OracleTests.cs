using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32K2OracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg32_k2");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg32K2_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0;
        ulong stream = testCase.Stream != null ? ulong.Parse(testCase.Stream) : 0;

        var rng = new Pcg32K2(seed, stream);

        foreach (var entry in testCase.Sequence)
        {
            uint actual = rng.Next();
            uint expected = entry.AsUInt32();
            Assert.Equal(expected, actual);
        }
    }
}

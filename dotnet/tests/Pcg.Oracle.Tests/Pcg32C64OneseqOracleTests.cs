using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32C64OneseqOracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg32_c64_oneseq");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg32C64Oneseq_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0UL;

        var rng = new Pcg32C64Oneseq(seed);

        foreach (var entry in testCase.Sequence)
        {
            uint actual = rng.Next();
            uint expected = entry.AsUInt32();
            Assert.Equal(expected, actual);
        }
    }
}

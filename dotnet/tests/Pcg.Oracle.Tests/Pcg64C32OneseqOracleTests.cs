using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64C32OneseqOracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg64_c32_oneseq");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg64C32Oneseq_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0;

        var rng = new Pcg64C32Oneseq(seed);

        foreach (var entry in testCase.Sequence)
        {
            ulong actual = rng.Next();
            ulong expected = entry.AsUInt64();
            Assert.Equal(expected, actual);
        }
    }
}

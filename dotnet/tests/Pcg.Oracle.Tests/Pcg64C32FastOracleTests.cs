using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64C32FastOracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg64_c32_fast");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg64C32Fast_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0;

        var rng = new Pcg64C32Fast(seed);

        foreach (var entry in testCase.Sequence)
        {
            ulong actual = rng.Next();
            ulong expected = entry.AsUInt64();
            Assert.Equal(expected, actual);
        }
    }
}

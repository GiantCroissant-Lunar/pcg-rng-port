using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg32C64OracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg32_c64");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg32C64_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0UL;
        ulong stream = testCase.Stream != null ? ulong.Parse(testCase.Stream) : 0UL;

        var rng = new Pcg32C64(seed, stream);

        foreach (var entry in testCase.Sequence)
        {
            uint actual = rng.Next();
            uint expected = entry.AsUInt32();
            Assert.Equal(expected, actual);
        }
    }
}

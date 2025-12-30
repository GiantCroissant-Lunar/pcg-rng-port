using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64K32OracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg64_k32");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg64K32_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0;
        ulong stream = testCase.Stream != null ? ulong.Parse(testCase.Stream) : 0;

        var rng = new Pcg64K32(seed, stream);

        foreach (var entry in testCase.Sequence)
        {
            ulong actual = rng.Next();
            ulong expected = entry.AsUInt64();
            Assert.Equal(expected, actual);
        }
    }
}

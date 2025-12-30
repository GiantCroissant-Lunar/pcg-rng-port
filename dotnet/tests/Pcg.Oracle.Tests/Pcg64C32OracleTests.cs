using Xunit;

namespace Pcg.Oracle.Tests;

public class Pcg64C32OracleTests
{
    public static IEnumerable<object[]> GetTestCases() => OracleDataLoader.Load("pcg64_c32");

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void Pcg64C32_MatchesOracle_Json(OracleTestCase testCase)
    {
        ulong seed = testCase.Seed != null ? ulong.Parse(testCase.Seed) : 0;
        ulong stream = testCase.Stream != null ? ulong.Parse(testCase.Stream) : 0;

        var rng = new Pcg64C32(seed, stream);

        foreach (var entry in testCase.Sequence)
        {
            ulong actual = rng.Next();
            ulong expected = entry.AsUInt64();
            Assert.Equal(expected, actual);
        }
    }
}
